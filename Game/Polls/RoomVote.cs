namespace DragonEmu.Game.Polls;

public sealed class RoomVote
{
    public static readonly string[] DefaultChoices = ["Yes", "No"];

    private readonly object _lock = new();
    private readonly HashSet<int> _participants;
    private readonly Dictionary<int, int> _answers = [];
    private bool _ended;

    public RoomVote(int roomId, int startedBy, string question, IEnumerable<int> participants)
    {
        RoomId = roomId;
        StartedBy = startedBy;
        Question = question;
        _participants = [.. participants];
    }

    public int RoomId { get; }

    public int StartedBy { get; }

    public string Question { get; }

    public IReadOnlyList<string> Choices => DefaultChoices;

    public IReadOnlyCollection<int> Participants
    {
        get
        {
            lock (_lock)
            {
                return [.. _participants];
            }
        }
    }

    public bool TryAnswer(int accountId, int choice)
    {
        lock (_lock)
        {
            if (_ended || choice < 1 || choice > Choices.Count || !_participants.Contains(accountId))
            {
                return false;
            }

            return _answers.TryAdd(accountId, choice);
        }
    }

    public int[]? End()
    {
        lock (_lock)
        {
            if (_ended)
            {
                return null;
            }

            _ended = true;
            var counts = new int[Choices.Count];
            foreach (int choice in _answers.Values)
            {
                counts[choice - 1]++;
            }

            return counts;
        }
    }
}
