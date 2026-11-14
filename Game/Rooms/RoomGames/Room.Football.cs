using DragonEmu.Domain;
using DragonEmu.Game.Rooms.Pathfinding;

namespace DragonEmu.Game.Rooms;

public readonly record struct BallMove(RoomFloorItem Ball, int X, int Y, double Z, int Direction, string State);

public readonly record struct BallRest(RoomFloorItem Ball, string State);

public readonly record struct FootballGoalScored(RoomFloorItem Goal, GameTeam Team, RoomActor? Kicker);

public readonly record struct CountdownFinished(int ItemId);

public sealed record FootballTickResult(
    IReadOnlyList<BallMove> Moves,
    IReadOnlyList<BallRest> Rests,
    IReadOnlyList<FootballGoalScored> Goals,
    IReadOnlyList<FurnitureStateChange> StateChanges,
    IReadOnlyList<CountdownFinished> Finished,
    bool NeedsFurtherTicks)
{
    public static FootballTickResult Idle { get; } = new([], [], [], [], [], false);
}

public sealed partial class Room
{
    private const long MaxLateMs = 50;

    private const string GoalScored = "1";
    private const string GoalIdle = "0";

    private readonly object _footballLock = new();
    private readonly Dictionary<int, BallRoll> _ballRolls = new();
    private readonly Dictionary<int, int> _goalPulses = new();
    private readonly Dictionary<int, FootballClock> _clocks = new();
    private int _clockParity;

    public bool FootballBusy
    {
        get
        {
            lock (_footballLock)
            {
                return _ballRolls.Count > 0 || _goalPulses.Count > 0 || _clocks.Values.Any(clock => clock.Running);
            }
        }
    }

    public bool BallRolling
    {
        get
        {
            lock (_footballLock)
            {
                return _ballRolls.Count > 0;
            }
        }
    }

    public bool TryKickBall(RoomFloorItem ball, int direction, RoomActor? kicker)
    {
        lock (_footballLock)
        lock (_furnitureLock)
        {
            if (!_floorItems.ContainsKey(ball.Id))
            {
                return false;
            }

            (GameTeam team, int? trail) = BallTrail(ball, kicker);
            _ballRolls[ball.Id] = new BallRoll(ball, direction, kicker?.SessionId, ball.X, ball.Y, team, trail);
            return true;
        }
    }

    public BallMove? TryNudgeBall(RoomFloorItem ball, int direction, HashSet<(int X, int Y)> occupied, int? trail = null)
    {
        lock (_footballLock)
        {
            _ballRolls.Remove(ball.Id);
            lock (_furnitureLock)
            {
                if (!_floorItems.ContainsKey(ball.Id) || BallDestinationUnlocked(ball, direction, occupied) is not { } to)
                {
                    return null;
                }

                return MoveBallUnlocked(ball, to.X, to.Y, to.Z, direction, FootballRules.RollState(FootballRules.RollingSlowly, trail));
            }
        }
    }

    public RoomFloorItem? BallAt(int x, int y)
    {
        lock (_furnitureLock)
        {
            return ItemsAtUnlocked(x, y).LastOrDefault(item => item.IsBall);
        }
    }

    private HashSet<(int X, int Y)> BallObstacleTiles(long excludeSessionId)
    {
        var tiles = new HashSet<(int X, int Y)>();
        foreach (RoomActor actor in _actors.Values)
        {
            if (actor.SessionId == excludeSessionId)
            {
                continue;
            }

            lock (actor.Lock)
            {
                tiles.Add(actor.PendingStep is { } step ? (step.X, step.Y) : (actor.X, actor.Y));
            }
        }

        return tiles;
    }

    public void SetClock(int itemId, int seconds, bool running)
    {
        lock (_footballLock)
        {
            int preset = FootballRules.CountdownPresets.Contains(seconds) ? seconds
                : _clocks.TryGetValue(itemId, out FootballClock? known) ? known.Preset : FootballRules.CountdownPresets[0];
            _clocks[itemId] = new FootballClock(seconds, running, preset);
        }
    }

    public int ClockPreset(int itemId)
    {
        lock (_footballLock)
        {
            return _clocks.TryGetValue(itemId, out FootballClock? clock) ? clock.Preset : FootballRules.CountdownPresets[0];
        }
    }

    public (int Seconds, bool Running)? GetClock(int itemId)
    {
        lock (_footballLock)
        {
            return _clocks.TryGetValue(itemId, out FootballClock? clock) ? (clock.Seconds, clock.Running) : null;
        }
    }

    public FootballTickResult AdvanceFootball(long nowMs, bool roomTick)
    {
        HashSet<(int X, int Y)>? occupied = null;
        lock (_footballLock)
        {
            if (_ballRolls.Count == 0 && _goalPulses.Count == 0 && _clocks.Count == 0)
            {
                return FootballTickResult.Idle;
            }
        }

        lock (_movementLock)
        {
            occupied = BallObstacleTiles(excludeSessionId: -1);

            lock (_footballLock)
            {
                var moves = new List<BallMove>();
                var rests = new List<BallRest>();
                var goals = new List<FootballGoalScored>();
                var stateChanges = new List<FurnitureStateChange>();
                var finished = new List<CountdownFinished>();

                lock (_furnitureLock)
                {
                    foreach ((int ballId, BallRoll roll) in _ballRolls.ToList())
                    {
                        if (!_floorItems.TryGetValue(ballId, out RoomFloorItem? ball) || ball.X != roll.LastX || ball.Y != roll.LastY)
                        {
                            _ballRolls.Remove(ballId);
                            continue;
                        }

                        if (nowMs < roll.DueAtMs)
                        {
                            continue;
                        }

                        AdvanceRoll(roll, ball, nowMs, occupied, moves, goals);
                        if (roll.Slot >= FootballRules.KickStates.Count)
                        {
                            _ballRolls.Remove(ballId);
                            rests.Add(new BallRest(ball, roll.LastState));
                        }
                    }
                }

                if (roomTick)
                {
                    foreach ((int goalId, int ticks) in _goalPulses.ToList())
                    {
                        if (ticks >= 1)
                        {
                            _goalPulses.Remove(goalId);
                            if (GetFloorItem(goalId) is not null)
                            {
                                stateChanges.Add(new FurnitureStateChange(goalId, GoalIdle));
                            }
                        }
                        else
                        {
                            _goalPulses[goalId] = ticks + 1;
                        }
                    }

                    _clockParity = (_clockParity + 1) % FootballRules.CountdownTicksPerSecond;
                    if (_clockParity == 0)
                    {
                        foreach ((int clockId, FootballClock clock) in _clocks.ToList())
                        {
                            if (GetFloorItem(clockId) is null)
                            {
                                _clocks.Remove(clockId);
                                continue;
                            }

                            if (!clock.Running)
                            {
                                continue;
                            }

                            clock.Seconds = Math.Max(0, clock.Seconds - 1);
                            stateChanges.Add(new FurnitureStateChange(clockId, clock.Seconds.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                            if (clock.Seconds == 0)
                            {
                                clock.Running = false;
                                finished.Add(new CountdownFinished(clockId));
                            }
                        }
                    }
                }

                foreach (FootballGoalScored goal in goals)
                {
                    _goalPulses[goal.Goal.Id] = 0;
                }

                bool busy = _ballRolls.Count > 0 || _goalPulses.Count > 0 || _clocks.Values.Any(clock => clock.Running);
                return new FootballTickResult(moves, rests, goals, stateChanges, finished, busy);
            }
        }
    }

    private void AdvanceRoll(BallRoll roll, RoomFloorItem ball, long nowMs, HashSet<(int X, int Y)> occupied, List<BallMove> moves, List<FootballGoalScored> goals)
    {
        while (roll.Slot < FootballRules.KickStates.Count)
        {
            string state = FootballRules.RollState(FootballRules.KickStates[roll.Slot], roll.Trail);
            roll.Slot++;

            if (BallDestinationUnlocked(ball, roll.Direction, occupied) is { } to)
            {
                moves.Add(MoveBallUnlocked(ball, to.X, to.Y, to.Z, roll.Direction, state));
                roll.LastX = to.X;
                roll.LastY = to.Y;
                roll.LastState = state;

                if (ball.IsBanzaiPuck && roll.Team != GameTeam.None)
                {
                    _puckPaints.Add((to.X, to.Y, roll.Team));
                }

                long scheduled = roll.Timed && nowMs - roll.DueAtMs <= MaxLateMs ? roll.DueAtMs : nowMs;
                roll.Timed = true;
                roll.DueAtMs = scheduled + FootballRules.IntervalMs(state);

                if (to.Goal is { } goal)
                {
                    goals.Add(new FootballGoalScored(goal, GameTeams.OfClassname(goal.Classname), roll.KickerSessionId is { } kicker && _actors.TryGetValue(kicker, out RoomActor? actor) ? actor : null));
                    roll.Direction = FootballRules.Opposite(roll.Direction);
                }

                return;
            }

            roll.Direction = FootballRules.Opposite(roll.Direction);
        }
    }

    private (int X, int Y, double Z, RoomFloorItem? Goal)? BallDestinationUnlocked(RoomFloorItem ball, int direction, HashSet<(int X, int Y)> occupied)
    {
        (int x, int y) = FootballRules.Step(ball.X, ball.Y, direction);
        if (!Grid.IsWalkable(x, y) || Grid.IsDoor(x, y) || occupied.Contains((x, y)))
        {
            return null;
        }

        double z = Grid.HeightAt(x, y);
        RoomFloorItem? goal = null;
        foreach (RoomFloorItem item in ItemsAtUnlocked(x, y))
        {
            if (item.IsBall)
            {
                if (item.Id != ball.Id)
                {
                    return null;
                }
            }
            else if (item.IsFootballGoal)
            {
                if (GoalMouth(item) != (x, y) || !FootballRules.EntersGoal(direction, item.Direction))
                {
                    return null;
                }

                goal = item;
                z = Math.Max(z, item.Z);
            }
            else if (!item.IsWalkable || item.IsBed || item.IsSittable)
            {
                return null;
            }
            else
            {
                z = Math.Max(z, item.SurfaceZ);
            }
        }

        return Math.Abs(z - ball.Z) > MaxClimbHeight ? null : (x, y, z, goal);
    }

    public static (int X, int Y) GoalMouth(RoomFloorItem goal)
    {
        List<(int X, int Y)> tiles = goal.OccupiedTiles().ToList();
        return tiles[tiles.Count / 2];
    }

    private BallMove MoveBallUnlocked(RoomFloorItem ball, int x, int y, double z, int direction, string state)
    {
        ball.MoveTo(x, y, Math.Round(z, 4), direction * 45);
        RebuildTileIndex();
        return new BallMove(ball, x, y, ball.Z, direction, state);
    }

    private BallMove? PlayBall(RoomActor actor, int nextX, int nextY, bool lastStep)
    {
        int direction = RotationHelper.Calculate(actor.X, actor.Y, nextX, nextY);

        if (BallAt(actor.X, actor.Y) is { } underfoot)
        {
            TryKickBall(underfoot, FootballRules.Opposite(direction), actor);
            return null;
        }

        if (BallAt(nextX, nextY) is not { } ball)
        {
            return null;
        }

        if (lastStep)
        {
            TryKickBall(ball, direction, actor);
            return null;
        }

        HashSet<(int X, int Y)> occupied = BallObstacleTiles(actor.SessionId);
        occupied.Add((nextX, nextY));
        return TryNudgeBall(ball, direction, occupied, BallTrail(ball, actor).Trail);
    }

    private static (GameTeam Team, int? Trail) BallTrail(RoomFloorItem ball, RoomActor? kicker)
    {
        if (!ball.IsBanzaiPuck)
        {
            return (GameTeam.None, null);
        }

        GameTeam team = kicker?.Team ?? GameTeam.None;
        return (team, BanzaiRules.PuckTrail(team));
    }

    public IReadOnlyList<BallMove> TakeBallNudges()
    {
        lock (_footballLock)
        {
            if (_nudges.Count == 0)
            {
                return [];
            }

            BallMove[] nudges = [.. _nudges];
            _nudges.Clear();
            return nudges;
        }
    }

    private readonly List<BallMove> _nudges = [];

    private sealed class BallRoll(RoomFloorItem ball, int direction, long? kickerSessionId, int lastX, int lastY, GameTeam team, int? trail)
    {
        public GameTeam Team { get; } = team;

        public int? Trail { get; } = trail;

        public RoomFloorItem Ball { get; } = ball;

        public int Direction { get; set; } = direction;

        public long? KickerSessionId { get; } = kickerSessionId;

        public int Slot { get; set; }

        public long DueAtMs { get; set; }

        public bool Timed { get; set; }

        public int LastX { get; set; } = lastX;

        public int LastY { get; set; } = lastY;

        public string LastState { get; set; } = FootballRules.RollState(FootballRules.RollingSlowly, trail);
    }

    private sealed class FootballClock(int seconds, bool running, int preset)
    {
        public int Seconds { get; set; } = seconds;

        public bool Running { get; set; } = running;

        public int Preset { get; } = preset;
    }
}
