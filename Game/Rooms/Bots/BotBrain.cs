namespace DragonEmu.Game.Rooms;

public sealed record BotGo(IReadOnlyList<BotStep> Steps, RoomActor? Target, bool FaceTarget);

public sealed class BotBrain
{
    public const int ReplyDelayTicks = 1;

    private readonly object _lock = new();
    private readonly Random _random;
    private readonly Dictionary<int, int> _randomTicksLeft = [];
    private readonly Dictionary<int, int> _lastAlternative = [];
    private readonly Dictionary<int, int> _cooldowns = [];
    private Run? _run;

    public BotBrain(BotDefinition definition, Random? random = null)
    {
        Definition = definition;
        _random = random ?? new Random();
        foreach (BotAction action in definition.Actions.Where(action => action.Trigger == BotTrigger.Random))
        {
            _randomTicksLeft[action.Id] = NextInterval(action);
        }

        if (definition.Actions.FirstOrDefault(action => action.Trigger == BotTrigger.OnArrival) is { } arrival)
        {
            _run = new Run(Pick(arrival), target: null, skipTicks: 0, faceTarget: false);
        }
    }

    public BotDefinition Definition { get; }

    public bool IsBusy
    {
        get
        {
            lock (_lock)
            {
                return _run is not null;
            }
        }
    }

    public bool Hear(RoomActor speaker, string message, bool shout, int distance)
    {
        BotTrigger trigger = shout ? BotTrigger.UserShouts : BotTrigger.UserSays;
        lock (_lock)
        {
            if (_run is not null || _cooldowns.ContainsKey(speaker.AccountId))
            {
                return false;
            }

            foreach (BotAction action in Definition.Actions)
            {
                if ((action.Trigger != trigger && action.Trigger != BotTrigger.UserChats) || (action.TileDistance > 0 && distance > action.TileDistance)
                    || action.Phrases?.Matches(message) != true)
                {
                    continue;
                }

                _run = new Run(Pick(action), speaker, skipTicks: ReplyDelayTicks, faceTarget: true);
                if (action.CooldownTicks > 0)
                {
                    _cooldowns[speaker.AccountId] = action.CooldownTicks;
                }

                return true;
            }

            return false;
        }
    }

    public BotGo? Tick(bool walking)
    {
        lock (_lock)
        {
            foreach (int userId in _cooldowns.Keys.ToList())
            {
                if (--_cooldowns[userId] <= 0)
                {
                    _cooldowns.Remove(userId);
                }
            }

            foreach (int actionId in _randomTicksLeft.Keys.ToList())
            {
                if (_randomTicksLeft[actionId] > 0)
                {
                    _randomTicksLeft[actionId]--;
                }
            }

            if (_run is null)
            {
                BotAction? due = Definition.Actions.FirstOrDefault(
                    action => action.Trigger == BotTrigger.Random && _randomTicksLeft[action.Id] == 0);
                if (due is null)
                {
                    return null;
                }

                _randomTicksLeft[due.Id] = NextInterval(due);
                _run = new Run(Pick(due), target: null, skipTicks: 0, faceTarget: false);
            }

            return Advance(walking);
        }
    }

    private BotGo? Advance(bool walking)
    {
        Run run = _run!;
        if (run.SkipTicks > 0)
        {
            run.SkipTicks--;
            return null;
        }

        if (run.WaitingForWalk && walking)
        {
            return null;
        }

        run.WaitingForWalk = false;
        bool face = run.FaceTarget;
        run.FaceTarget = false;

        var steps = new List<BotStep>();
        while (run.Index < run.Steps.Count)
        {
            BotStep step = run.Steps[run.Index++];
            if (step.Type == BotStepType.Wait)
            {
                run.SkipTicks = step.Value - 1;
                break;
            }

            steps.Add(step);
            if (step.Type == BotStepType.Walk && run.Index < run.Steps.Count)
            {
                run.WaitingForWalk = true;
                break;
            }
        }

        RoomActor? target = run.Target;
        if (run.Index >= run.Steps.Count && run.SkipTicks == 0 && !run.WaitingForWalk)
        {
            _run = null;
        }

        return steps.Count > 0 || face ? new BotGo(steps, target, face) : null;
    }

    private IReadOnlyList<BotStep> Pick(BotAction action)
    {
        int count = action.Alternatives.Count;
        int index = 0;
        if (count > 1 && _lastAlternative.TryGetValue(action.Id, out int last))
        {
            index = _random.Next(count - 1);
            if (index >= last)
            {
                index++;
            }
        }
        else if (count > 1)
        {
            index = _random.Next(count);
        }

        _lastAlternative[action.Id] = index;
        return action.Alternatives[index];
    }

    private int NextInterval(BotAction action) => _random.Next(action.MinTicks, action.MaxTicks + 1);

    private sealed class Run(IReadOnlyList<BotStep> steps, RoomActor? target, int skipTicks, bool faceTarget)
    {
        public IReadOnlyList<BotStep> Steps { get; } = steps;

        public RoomActor? Target { get; } = target;

        public int Index { get; set; }

        public int SkipTicks { get; set; } = skipTicks;

        public bool FaceTarget { get; set; } = faceTarget;

        public bool WaitingForWalk { get; set; }
    }
}
