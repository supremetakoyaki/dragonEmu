using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public sealed class IdolTickResult
{
    public static readonly IdolTickResult None = new();

    public List<long> JudgesSat { get; } = [];

    public List<long> JudgesLeft { get; } = [];

    public long? PerformerArrived { get; set; }

    public long? PerformerLeft { get; set; }

    public bool PerformanceEnded { get; set; }

    public List<FurnitureStateChange> StateChanges { get; } = [];
}

public sealed partial class Room
{
    public static readonly TimeSpan ChairLitFor = TimeSpan.FromSeconds(2);

    public const string HotspotOff = "-1";
    public const string ChairUnlit = "0";
    public const string ChairLitYes = "1";
    public const string ChairLitNo = "2";

    private readonly object _idolLock = new();
    private readonly Dictionary<long, int> _judges = [];
    private readonly HashSet<long> _voted = [];
    private readonly Dictionary<int, DateTime> _litChairs = [];
    private long? _performer;
    private bool _performing;
    private DateTime? _performanceEndsAt;
    private int _votes;

    public RoomFloorItem? Hotspot => FloorItemsWhere(item => item.LogicClass == FurnitureLogicClass.Hotspot).FirstOrDefault();

    public bool IdolBusy
    {
        get
        {
            lock (_idolLock)
            {
                return _performing || _litChairs.Count > 0;
            }
        }
    }

    public long? Performer
    {
        get
        {
            lock (_idolLock)
            {
                return _performer;
            }
        }
    }

    public bool IsPerforming
    {
        get
        {
            lock (_idolLock)
            {
                return _performing;
            }
        }
    }

    public IReadOnlyList<(long SessionId, bool Voted)> Judges
    {
        get
        {
            lock (_idolLock)
            {
                return _judges.Keys.Select(id => (id, _voted.Contains(id))).ToList();
            }
        }
    }

    public IdolTickResult AdvanceIdol(DateTime utcNow, List<RoomActor> changed)
    {
        RoomFloorItem? hotspot = Hotspot;
        Dictionary<(int X, int Y), RoomFloorItem> chairs = FloorItemsWhere(item => item.LogicClass == FurnitureLogicClass.VotingChair)
            .ToDictionary(chair => (chair.X, chair.Y));

        lock (_idolLock)
        {
            if (hotspot is null && chairs.Count == 0 && _judges.Count == 0 && _performer is null && _litChairs.Count == 0)
            {
                return IdolTickResult.None;
            }

            var result = new IdolTickResult();
            var sitting = new Dictionary<long, int>();
            RoomActor? onHotspot = null;
            foreach (RoomActor actor in _actors.Values)
            {
                if (actor.Type != ActorType.Player)
                {
                    continue;
                }

                lock (actor.Lock)
                {
                    if (actor.IsWalking || actor.PendingStep is not null)
                    {
                        continue;
                    }

                    if (actor.Posture == ActorPosture.Sit && chairs.TryGetValue((actor.X, actor.Y), out RoomFloorItem? chair))
                    {
                        sitting[actor.SessionId] = chair.Id;
                    }
                    else if (hotspot is not null && actor.X == hotspot.X && actor.Y == hotspot.Y
                        && (onHotspot is null || actor.SessionId == _performer))
                    {
                        onHotspot = actor;
                    }
                }
            }

            foreach (long left in _judges.Keys.Where(id => !sitting.ContainsKey(id)).ToList())
            {
                _judges.Remove(left);
                result.JudgesLeft.Add(left);
            }

            foreach ((long sessionId, int chairId) in sitting)
            {
                if (_judges.TryAdd(sessionId, chairId))
                {
                    result.JudgesSat.Add(sessionId);
                }
            }

            if (_performer is { } performer && onHotspot?.SessionId != performer)
            {
                _performer = null;
                result.PerformerLeft = performer;
                result.PerformanceEnded = EndPerformanceUnlocked();
            }

            if (_performer is null && onHotspot is not null)
            {
                _performer = onHotspot.SessionId;
                result.PerformerArrived = onHotspot.SessionId;
                lock (onHotspot.Lock)
                {
                    int facing = (hotspot!.Rotation / 45) % 8;
                    if (onHotspot.RotBody != facing || onHotspot.RotHead != facing)
                    {
                        onHotspot.RotBody = facing;
                        onHotspot.RotHead = facing;
                        if (!changed.Contains(onHotspot))
                        {
                            changed.Add(onHotspot);
                        }
                    }
                }
            }

            if (_performing && _performanceEndsAt is { } endsAt && utcNow >= endsAt)
            {
                result.PerformanceEnded = EndPerformanceUnlocked();
            }

            foreach ((int chairId, DateTime until) in _litChairs.Where(lit => utcNow >= lit.Value).ToList())
            {
                _litChairs.Remove(chairId);
                result.StateChanges.Add(new FurnitureStateChange(chairId, ChairUnlit));
            }

            return result;
        }
    }

    public bool TryStartPerformance(long sessionId, TimeSpan? length, DateTime utcNow)
    {
        lock (_idolLock)
        {
            if (_performer != sessionId || _performing)
            {
                return false;
            }

            _performing = true;
            _performanceEndsAt = length is { } song ? utcNow + song : null;
            _votes = 0;
            _voted.Clear();
            return true;
        }
    }

    public (int Votes, int ChairId, string ChairState)? TryVote(long sessionId, bool yes, DateTime utcNow)
    {
        lock (_idolLock)
        {
            if (!_performing || !_judges.TryGetValue(sessionId, out int chairId) || !_voted.Add(sessionId))
            {
                return null;
            }

            _votes = yes ? Math.Min(_votes + 1, 99) : Math.Max(_votes - 1, 0);
            _litChairs[chairId] = utcNow + ChairLitFor;
            return (_votes, chairId, yes ? ChairLitYes : ChairLitNo);
        }
    }

    private bool EndPerformanceUnlocked()
    {
        if (!_performing)
        {
            return false;
        }

        _performing = false;
        _performanceEndsAt = null;
        _voted.Clear();
        return true;
    }
}
