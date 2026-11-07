using System.Collections.Concurrent;
using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public sealed record RoomQueueTickResult(
    IReadOnlyList<(long SessionId, RoomQueueLine Line)> Admitted,
    IReadOnlyList<(long SessionId, RoomQueueStatus Status)> Moved,
    bool NeedsFurtherTicks);

public sealed partial class Room
{
    public static readonly TimeSpan AdmissionWindow = TimeSpan.FromSeconds(30);

    private readonly object _queueLock = new();
    private readonly List<(long SessionId, bool Club, RoomQueueLine Line)> _queue = [];
    private readonly Dictionary<long, (RoomQueueLine Line, DateTime ExpiresAtUtc)> _admitted = [];
    private readonly ConcurrentDictionary<long, byte> _spectators = new();
    private bool _queueMoved;

    public IReadOnlyCollection<long> Spectators => _spectators.Keys.ToArray();

    public bool HasQueueOrSpectators
    {
        get
        {
            lock (_queueLock)
            {
                return _queue.Count > 0 || _admitted.Count > 0 || !_spectators.IsEmpty;
            }
        }
    }

    public bool MustQueue(RoomQueueLine line, int capacity)
    {
        lock (_queueLock)
        {
            return Taken(line) >= capacity || _queue.Exists(waiting => waiting.Line == line);
        }
    }

    public RoomQueueStatus JoinQueue(long sessionId, bool club, RoomQueueLine line)
    {
        lock (_queueLock)
        {
            _queue.RemoveAll(waiting => waiting.SessionId == sessionId);
            _queue.Add((sessionId, club, line));
            _queueMoved = true;
            return StatusOf(sessionId)!;
        }
    }

    public RoomQueueStatus? ChangeQueueLine(long sessionId, RoomQueueLine line)
    {
        lock (_queueLock)
        {
            int index = _queue.FindIndex(waiting => waiting.SessionId == sessionId);
            if (index < 0)
            {
                return null;
            }

            return _queue[index].Line == line ? StatusOf(sessionId) : JoinQueue(sessionId, _queue[index].Club, line);
        }
    }

    public bool LeaveQueue(long sessionId)
    {
        lock (_queueLock)
        {
            bool left = _queue.RemoveAll(waiting => waiting.SessionId == sessionId) > 0 | _admitted.Remove(sessionId);
            _queueMoved |= left;
            return left;
        }
    }

    public RoomQueueLine? TakeAdmission(long sessionId)
    {
        lock (_queueLock)
        {
            return _admitted.Remove(sessionId, out var held) ? held.Line : null;
        }
    }

    public void StartSpectating(long sessionId) => _spectators.TryAdd(sessionId, 0);

    public RoomQueueTickResult AdvanceQueue(int maxUsers, int maxSpectators, DateTime utcNow)
    {
        lock (_queueLock)
        {
            foreach (long expired in _admitted.Where(held => held.Value.ExpiresAtUtc <= utcNow).Select(held => held.Key).ToList())
            {
                _admitted.Remove(expired);
                _queueMoved = true;
            }

            var admitted = new List<(long, RoomQueueLine)>();
            foreach ((RoomQueueLine line, int capacity) in new[] { (RoomQueueLine.Visitors, maxUsers), (RoomQueueLine.Spectators, maxSpectators) })
            {
                while (Taken(line) < capacity && Ordered(line).Take(1).ToList() is [var next])
                {
                    _queue.Remove(next);
                    _admitted[next.SessionId] = (line, utcNow + AdmissionWindow);
                    admitted.Add((next.SessionId, line));
                    _queueMoved = true;
                }
            }

            var moved = new List<(long, RoomQueueStatus)>();
            if (_queueMoved)
            {
                moved.AddRange(_queue.Select(waiting => (waiting.SessionId, StatusOf(waiting.SessionId)!)));
                _queueMoved = false;
            }

            return new RoomQueueTickResult(admitted, moved, _queue.Count > 0 || _admitted.Count > 0);
        }
    }

    private int Taken(RoomQueueLine line) =>
        (line == RoomQueueLine.Visitors ? UserCount : _spectators.Count) + _admitted.Values.Count(held => held.Line == line);

    private IEnumerable<(long SessionId, bool Club, RoomQueueLine Line)> Ordered(RoomQueueLine line) =>
        _queue.Where(waiting => waiting.Line == line).OrderByDescending(waiting => waiting.Club);

    private RoomQueueStatus? StatusOf(long sessionId)
    {
        int index = _queue.FindIndex(waiting => waiting.SessionId == sessionId);
        if (index < 0)
        {
            return null;
        }

        (_, bool club, RoomQueueLine line) = _queue[index];
        List<long> order = Ordered(line).Select(waiting => waiting.SessionId).ToList();
        int clubAhead = _queue.Take(index).Count(waiting => waiting.Line == line && waiting.Club);

        RoomQueueLine otherLine = line == RoomQueueLine.Visitors ? RoomQueueLine.Spectators : RoomQueueLine.Visitors;
        int clubThere = _queue.Count(waiting => waiting.Line == otherLine && waiting.Club);
        int allThere = _queue.Count(waiting => waiting.Line == otherLine);
        return new RoomQueueStatus(
            new RoomQueuePlace(line, order.IndexOf(sessionId), club ? clubAhead : null),
            new RoomQueuePlace(otherLine, club ? clubThere : allThere, club ? clubThere : null));
    }

    private bool StopSpectating(long sessionId) => _spectators.TryRemove(sessionId, out _);
}
