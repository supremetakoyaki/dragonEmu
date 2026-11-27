using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public readonly record struct FreezeChange(int ItemId, string Data, bool Save);

public readonly record struct FreezePlayerResult(int AccountId, GameTeam Team, int TeamScore, bool Won);

public sealed record FreezeGameResult(IReadOnlyDictionary<int, int> Freezes, IReadOnlyList<FreezePlayerResult> Players);

public sealed record FreezeTickResult(
    IReadOnlyList<FreezeChange> Changes,
    IReadOnlyList<(long SessionId, bool Playing)> Playing,
    IReadOnlyList<(int VirtualId, int Lives)> Lives,
    IReadOnlyList<int> Waves,
    IReadOnlyList<int> PowerUps,
    int? StoppedClockId,
    IReadOnlyList<FreezeGameResult> Games,
    bool Busy)
{
    public static FreezeTickResult Idle { get; } = new([], [], [], [], [], null, [], false);

    public bool IsEmpty => Changes.Count == 0 && Playing.Count == 0 && Lives.Count == 0 && Waves.Count == 0 && PowerUps.Count == 0
        && StoppedClockId is null && Games.Count == 0;
}

public enum FreezePhase
{
    Idle,

    Running,

    Paused,

    Ending,

    Over,
}

public sealed partial class Room
{
    private readonly object _freezeLock = new();
    private readonly Dictionary<GameTeam, HashSet<long>> _freezeTeams = FreezeRules.Teams.ToDictionary(team => team, _ => new HashSet<long>());
    private readonly Dictionary<GameTeam, int> _freezeScores = FreezeRules.Teams.ToDictionary(team => team, _ => 0);
    private readonly Dictionary<long, FreezePlayer> _freezePlayers = [];
    private readonly List<Snowball> _snowballs = [];
    private readonly List<FreezeChange> _freezeChanges = [];
    private readonly List<(long SessionId, bool Playing)> _freezePlaying = [];
    private readonly List<(int VirtualId, int Lives)> _freezeLives = [];
    private readonly List<int> _freezeWaves = [];
    private readonly List<int> _freezePowerUps = [];
    private readonly List<FreezeGameResult> _freezeGames = [];

    private readonly List<FreezeExitTrip> _freezeExits = [];

    private readonly Dictionary<long, (int AccountId, GameTeam Team)> _freezeParticipants = [];
    private readonly HashSet<int> _freezeOutAccounts = [];
    private readonly Dictionary<int, int> _freezeFreezes = [];

    private FreezePhase _freezePhase;
    private int _freezeClockId;
    private int _freezeTick;
    private int _freezeTicksLeft;
    private int _freezeTeamsAtStart;
    private bool _freezeWithLives;
    private int? _freezeStopClock;
    private volatile bool _freezeInProgress;
    private volatile bool _freezeGatesShut;

    private volatile bool _hasFreeze;

    public Func<int, int> FreezeRoll { get; set; } = Random.Shared.Next;

    public FreezePhase FreezePhase
    {
        get
        {
            lock (_freezeLock)
            {
                return _freezePhase;
            }
        }
    }

    public bool FreezeInProgress => _freezeInProgress;

    public bool FreezeBusy
    {
        get
        {
            lock (_freezeLock)
            {
                return _freezePhase is FreezePhase.Running or FreezePhase.Ending || _freezeExits.Count > 0 || _freezeChanges.Count > 0
                    || _freezePlaying.Count > 0 || _freezeLives.Count > 0 || _freezePowerUps.Count > 0 || _freezeGames.Count > 0;
            }
        }
    }

    public int FreezePlayers(GameTeam team)
    {
        lock (_freezeLock)
        {
            return _freezeTeams[team].Count;
        }
    }

    public int FreezeScore(GameTeam team)
    {
        lock (_freezeLock)
        {
            return _freezeScores.GetValueOrDefault(team);
        }
    }

    public int? FreezeLives(long sessionId)
    {
        lock (_freezeLock)
        {
            return _freezePlayers.TryGetValue(sessionId, out FreezePlayer? player) ? player.Lives : null;
        }
    }

    public static string FreezeShownState(RoomFloorItem item) => item switch
    {
        { IsFreezeTile: true } or { IsFreezeBlock: true } => FreezeRules.Shown(item.TileState),
        _ => item.TileState.ToString(System.Globalization.CultureInfo.InvariantCulture),
    };

    private void NoteFreezeItems() =>
        _hasFreeze = _floorItems.Values.Any(item => item.IsFreezeTile || item.IsFreezeBlock || item.IsFreezeGate);

    private bool IsShutFreezeGate(int x, int y)
    {
        if (!_freezeGatesShut)
        {
            return false;
        }

        lock (_furnitureLock)
        {
            return ItemsAtUnlocked(x, y).Any(item => item.IsFreezeGate);
        }
    }

    private HashSet<(int X, int Y)> FreezeGateTiles()
    {
        var tiles = new HashSet<(int X, int Y)>();
        lock (_furnitureLock)
        {
            foreach (RoomFloorItem gate in _floorItems.Values)
            {
                if (gate.IsFreezeGate)
                {
                    tiles.UnionWith(gate.OccupiedTiles());
                }
            }
        }

        return tiles;
    }

    private void PlayFreeze(RoomActor actor, int x, int y)
    {
        if (actor.Type != ActorType.Player || !_hasFreeze)
        {
            return;
        }

        RoomFloorItem? top;
        lock (_furnitureLock)
        {
            top = TopUnderBallsUnlocked(x, y);
        }

        if (top is { IsFreezeGate: true })
        {
            UseFreezeGate(actor, GameTeams.OfClassname(top.Classname));
        }
    }

    private void UseFreezeGate(RoomActor actor, GameTeam team)
    {
        if (team == GameTeam.None)
        {
            return;
        }

        if (actor.Team != GameTeam.None && !actor.InFreeze)
        {
            LeaveBanzaiTeam(actor);
        }

        lock (_freezeLock)
        {
            if (actor.InFreeze && actor.Team == team)
            {
                LeaveFreezeTeamUnlocked(actor);
                _freezeParticipants.Remove(actor.SessionId);
                actor.Team = GameTeam.None;
                actor.InFreeze = false;
                actor.FrozenInGame = false;
                SetGameEffect(actor, 0);
                _freezePlaying.Add((actor.SessionId, false));
                return;
            }

            if (_freezeTeams[team].Count >= FreezeRules.MaxPlayersPerTeam || (_freezeInProgress && _freezeOutAccounts.Contains(actor.AccountId)))
            {
                return;
            }

            bool wasPlaying = actor.InFreeze;
            if (wasPlaying)
            {
                LeaveFreezeTeamUnlocked(actor);
            }

            _freezeTeams[team].Add(actor.SessionId);
            var player = new FreezePlayer(actor.SessionId, actor.AccountId, actor.VirtualId, team);
            _freezePlayers[actor.SessionId] = player;
            actor.Team = team;
            actor.InFreeze = true;
            actor.FrozenInGame = false;
            SetGameEffect(actor, FreezeRules.EffectOf(team));
            QueueFreezeGates(team);
            if (!wasPlaying)
            {
                _freezePlaying.Add((actor.SessionId, true));
            }

            if (_freezeInProgress)
            {
                _freezeParticipants[actor.SessionId] = (actor.AccountId, team);
                if (_freezeWithLives)
                {
                    player.Lives = FreezeRules.StartLives;
                    _freezeLives.Add((actor.VirtualId, player.Lives));
                    RecountFreezeScoresUnlocked();
                }
            }
        }
    }

    private void LeaveFreezeTeamUnlocked(RoomActor actor)
    {
        foreach (GameTeam team in FreezeRules.Teams)
        {
            if (_freezeTeams[team].Remove(actor.SessionId))
            {
                QueueFreezeGates(team);
            }
        }

        if (_freezePlayers.Remove(actor.SessionId, out FreezePlayer? player))
        {
            foreach (Snowball ball in _snowballs.Where(ball => ball.Owner == player.SessionId))
            {
                ball.Owner = 0;
            }

            if (_freezeWithLives && _freezeInProgress)
            {
                RecountFreezeScoresUnlocked();
            }
        }
    }

    private void LeaveFreeze(RoomActor actor)
    {
        lock (actor.Lock)
        {
            lock (_freezeLock)
            {
                _freezeExits.RemoveAll(trip => trip.SessionId == actor.SessionId);
                _freezeParticipants.Remove(actor.SessionId);
                if (!actor.InFreeze)
                {
                    return;
                }

                LeaveFreezeTeamUnlocked(actor);
                _freezePlaying.Add((actor.SessionId, false));
            }

            actor.Team = GameTeam.None;
            actor.InFreeze = false;
            actor.FrozenInGame = false;
        }
    }

    private void QueueFreezeGates(GameTeam team)
    {
        int count = _freezeTeams[team].Count;
        lock (_furnitureLock)
        {
            foreach (RoomFloorItem gate in _floorItems.Values)
            {
                if (gate.IsFreezeGate && GameTeams.OfClassname(gate.Classname) == team)
                {
                    gate.TileState = count;
                    QueueFreeze(gate.Id, count.ToString(System.Globalization.CultureInfo.InvariantCulture), save: true);
                }
            }
        }
    }

    private void QueueFreeze(int itemId, string data, bool save) => _freezeChanges.Add(new FreezeChange(itemId, data, save));

    public void StartFreeze(int clockId)
    {
        lock (_movementLock)
        {
            var updates = new List<FreezeActorUpdate>();
            lock (_freezeLock)
            {
                if (_freezeInProgress)
                {
                    RecordFreezeGameUnlocked(winners: null);
                }

                _freezePhase = FreezePhase.Running;
                _freezeClockId = clockId;
                _freezeTick = 0;
                _freezeInProgress = true;
                _freezeGatesShut = true;
                _freezeStopClock = null;
                _snowballs.Clear();
                _freezeOutAccounts.Clear();
                _freezeFreezes.Clear();
                _freezeParticipants.Clear();

                lock (_furnitureLock)
                {
                    _freezeWithLives = _floorItems.Values.Any(item => item.IsFreezeExit);
                    foreach (RoomFloorItem item in _floorItems.Values)
                    {
                        if (item.IsFreezeTile)
                        {
                            item.TileState = FreezeRules.TilePlain;
                            QueueFreeze(item.Id, FreezeRules.Shown(FreezeRules.TilePlain, FreezeRoll(5)), save: false);
                        }
                        else if (item.IsFreezeBlock)
                        {
                            item.TileState = FreezeRules.BlockWhole;
                            QueueFreeze(item.Id, FreezeRules.Shown(FreezeRules.BlockWhole), save: false);
                        }
                        else if (item.IsFreezeExit)
                        {
                            item.TileState = FreezeRules.ExitLit;
                            QueueFreeze(item.Id, FreezeRules.ExitLit.ToString(System.Globalization.CultureInfo.InvariantCulture), save: false);
                        }
                    }
                }

                _freezeTeamsAtStart = FreezeRules.Teams.Count(team => _freezeTeams[team].Count > 0);
                foreach (FreezePlayer player in _freezePlayers.Values)
                {
                    player.Reset(_freezeWithLives ? FreezeRules.StartLives : 0);
                    _freezeParticipants[player.SessionId] = (player.AccountId, player.Team);
                    if (_freezeWithLives)
                    {
                        _freezeLives.Add((player.VirtualId, player.Lives));
                    }

                    updates.Add(new FreezeActorUpdate(player.SessionId, FreezeRules.EffectOf(player.Team), Frozen: false));
                }

                foreach (GameTeam team in FreezeRules.Teams)
                {
                    _freezeScores[team] = -1;
                }

                RecountFreezeScoresUnlocked(force: true);
            }

            ApplyFreezeUpdates(updates, changed: null);
        }
    }

    public bool PauseFreeze()
    {
        lock (_freezeLock)
        {
            if (_freezePhase != FreezePhase.Running)
            {
                return false;
            }

            _freezePhase = FreezePhase.Paused;
            _freezeGatesShut = false;
            return true;
        }
    }

    public bool ResumeFreeze(int clockId)
    {
        lock (_freezeLock)
        {
            if (_freezePhase != FreezePhase.Paused)
            {
                return false;
            }

            _freezePhase = FreezePhase.Running;
            _freezeClockId = clockId;
            _freezeGatesShut = true;
            return true;
        }
    }

    public bool StopFreeze()
    {
        lock (_movementLock)
        {
            var updates = new List<FreezeActorUpdate>();
            lock (_freezeLock)
            {
                if (_freezePhase != FreezePhase.Paused)
                {
                    return false;
                }

                EndFreezeUnlocked(ceremony: false, updates);
            }

            ApplyFreezeUpdates(updates, changed: null);
            return true;
        }
    }

    public bool TryThrowSnowball(RoomActor actor, int itemId)
    {
        lock (actor.Lock)
        {
            if (!actor.InFreeze || actor.FrozenInGame)
            {
                return false;
            }

            (int px, int py) = actor.PendingStep is { } step ? (step.X, step.Y) : (actor.X, actor.Y);
            lock (_freezeLock)
            {
                if (_freezePhase != FreezePhase.Running || !_freezePlayers.TryGetValue(actor.SessionId, out FreezePlayer? player)
                    || player.FrozenTicks > 0 || player.BallsInAir >= player.MaxBalls
                    || (player.MaxBalls == 1 && _freezeTick < player.ReadyAtTick))
                {
                    return false;
                }

                RoomFloorItem? tile;
                lock (_furnitureLock)
                {
                    if (!_floorItems.TryGetValue(itemId, out RoomFloorItem? item))
                    {
                        return false;
                    }

                    tile = item.IsFreezeTile ? item
                        : item.IsFreezeBlock && !FreezeRules.IsWhole(item.TileState) ? FreezeTileAtUnlocked(item.X, item.Y)
                        : null;
                    if (tile is null || FreezeBlockAtUnlocked(tile.X, tile.Y) is { } block && FreezeRules.IsWhole(block.TileState)
                        || !FreezeRules.InThrowingReach(px, py, tile.X, tile.Y) || _snowballs.Any(ball => ball.TileId == tile.Id))
                    {
                        return false;
                    }
                }

                FreezeBallKind kind = player.NextBall;
                player.NextBall = FreezeBallKind.Normal;
                player.BallsInAir++;
                _snowballs.Add(new Snowball(tile.Id, tile.X, tile.Y, player.SessionId, player.AccountId, player.Team, kind, player.Reach));
                return true;
            }
        }
    }

    private void TakeFreezePowerUp(RoomActor actor, int x, int y)
    {
        if (!actor.InFreeze || !_hasFreeze)
        {
            return;
        }

        lock (_freezeLock)
        {
            if (_freezePhase != FreezePhase.Running || !_freezePlayers.TryGetValue(actor.SessionId, out FreezePlayer? player))
            {
                return;
            }

            int powerUp;
            lock (_furnitureLock)
            {
                if (FreezeBlockAtUnlocked(x, y) is not { } block || !FreezeRules.IsPowerUp(block.TileState))
                {
                    return;
                }

                powerUp = block.TileState;
                block.TileState = FreezeRules.Taken(powerUp);
                QueueFreeze(block.Id, FreezeRules.Shown(block.TileState), save: false);
            }

            _freezePowerUps.Add(player.AccountId);
            switch (powerUp)
            {
                case FreezeRules.PowerUpBiggerBlast:
                    player.Reach++;
                    break;
                case FreezeRules.PowerUpMoreSnowballs:
                    player.MaxBalls++;
                    break;
                case FreezeRules.PowerUpDiagonalBlast:
                    player.NextBall = FreezeBallKind.Diagonal;
                    break;
                case FreezeRules.PowerUpMegaSnowball:
                    player.NextBall = FreezeBallKind.Mega;
                    break;
                case FreezeRules.PowerUpLife:
                    if (_freezeWithLives && player.Lives < FreezeRules.MaxLives)
                    {
                        player.Lives++;
                        _freezeLives.Add((player.VirtualId, player.Lives));
                        RecountFreezeScoresUnlocked();
                    }

                    break;
                case FreezeRules.PowerUpShield:
                    player.ShieldTicks = Math.Max(player.ShieldTicks, FreezeRules.ShieldPowerUpTicks);
                    SetGameEffect(actor, FreezeRules.ShieldEffectOf(player.Team));
                    break;
            }
        }
    }

    public FreezeTickResult AdvanceFreeze(IReadOnlyList<CountdownFinished> finishedClocks, List<RoomActor>? changed = null)
    {
        if (!_hasFreeze && !_freezeInProgress && !FreezeBusy)
        {
            return FreezeTickResult.Idle;
        }

        lock (_movementLock)
        {
            long[] sessions;
            lock (_freezeLock)
            {
                sessions = [.. _freezePlayers.Keys];
            }

            var positions = new Dictionary<long, (int X, int Y)>();
            foreach (long sessionId in sessions)
            {
                if (_actors.TryGetValue(sessionId, out RoomActor? actor))
                {
                    lock (actor.Lock)
                    {
                        positions[sessionId] = actor.PendingStep is { } step ? (step.X, step.Y) : (actor.X, actor.Y);
                    }
                }
            }

            var updates = new List<FreezeActorUpdate>();
            FreezeTickResult result;
            lock (_freezeLock)
            {
                AdvanceFreezeExitsUnlocked(updates);

                switch (_freezePhase)
                {
                    case FreezePhase.Running:
                        if (GetFloorItem(_freezeClockId) is null)
                        {
                            EndFreezeUnlocked(ceremony: false, updates);
                            break;
                        }

                        _freezeTick++;
                        AdvanceFreezePlayersUnlocked(updates);
                        AdvanceSnowballsUnlocked(positions, updates);
                        if (finishedClocks.Any(clock => clock.ItemId == _freezeClockId))
                        {
                            _freezePhase = FreezePhase.Ending;
                            _freezeTicksLeft = FreezeRules.EndDelayTicks;
                        }
                        else if (IsLastTeamStandingUnlocked())
                        {
                            _freezeStopClock = _freezeClockId;
                            EndFreezeUnlocked(ceremony: true, updates);
                        }

                        break;

                    case FreezePhase.Ending:
                        if (--_freezeTicksLeft <= 0)
                        {
                            EndFreezeUnlocked(ceremony: true, updates);
                        }

                        break;
                }

                result = HandOutFreezeUnlocked();
            }

            ApplyFreezeUpdates(updates, changed);
            return result;
        }
    }

    private void AdvanceFreezePlayersUnlocked(List<FreezeActorUpdate> updates)
    {
        foreach (FreezePlayer player in _freezePlayers.Values.ToList())
        {
            if (player.FrozenTicks > 0)
            {
                if (--player.FrozenTicks > 0)
                {
                    continue;
                }

                if (_freezeWithLives && player.Lives <= 0)
                {
                    PutOutUnlocked(player, updates);
                    continue;
                }

                player.ShieldTicks = FreezeRules.ShieldTicks;
                updates.Add(new FreezeActorUpdate(player.SessionId, FreezeRules.ShieldEffectOf(player.Team), Frozen: false));
            }
            else if (player.ShieldTicks > 0 && --player.ShieldTicks == 0)
            {
                updates.Add(new FreezeActorUpdate(player.SessionId, FreezeRules.EffectOf(player.Team)));
            }
        }
    }

    private void PutOutUnlocked(FreezePlayer player, List<FreezeActorUpdate> updates)
    {
        _freezePlayers.Remove(player.SessionId);
        if (_freezeTeams[player.Team].Remove(player.SessionId))
        {
            QueueFreezeGates(player.Team);
        }

        _freezeOutAccounts.Add(player.AccountId);
        _freezePlaying.Add((player.SessionId, false));
        _freezeExits.Add(new FreezeExitTrip(player.SessionId, FreezeRules.ExitTeleportTicks, FreezeRules.OutEffectTicks));
        updates.Add(new FreezeActorUpdate(player.SessionId, FreezeRules.OutEffect, Frozen: false, LeaveTeam: true));
        RecountFreezeScoresUnlocked();
    }

    private void AdvanceFreezeExitsUnlocked(List<FreezeActorUpdate> updates)
    {
        for (int i = _freezeExits.Count - 1; i >= 0; i--)
        {
            FreezeExitTrip trip = _freezeExits[i];
            if (trip.TicksToExit > 0)
            {
                trip.TicksToExit--;
                if (trip.TicksToExit == 0)
                {
                    RoomFloorItem? exit;
                    lock (_furnitureLock)
                    {
                        exit = _floorItems.Values.Where(item => item.IsFreezeExit).OrderBy(item => item.Id).FirstOrDefault();
                    }

                    if (exit is not null)
                    {
                        updates.Add(new FreezeActorUpdate(trip.SessionId, TeleportTo: (exit.X, exit.Y)));
                    }
                }

                continue;
            }

            if (--trip.TicksToClear <= 0)
            {
                updates.Add(new FreezeActorUpdate(trip.SessionId, 0, OnlyOutOfTeam: true));
                _freezeExits.RemoveAt(i);
            }
        }
    }

    private void AdvanceSnowballsUnlocked(IReadOnlyDictionary<long, (int X, int Y)> positions, List<FreezeActorUpdate> updates)
    {
        var due = new List<Snowball>();
        foreach (Snowball ball in _snowballs)
        {
            if (ball.Shown && --ball.FuseLeft <= 0)
            {
                due.Add(ball);
            }
        }

        lock (_furnitureLock)
        {
            if (due.Count > 0)
            {
                ExplodeUnlocked(due, positions, updates);
            }

            foreach (Snowball ball in _snowballs)
            {
                if (!ball.Shown)
                {
                    ball.Shown = true;
                    if (_floorItems.TryGetValue(ball.TileId, out RoomFloorItem? tile))
                    {
                        tile.TileState = ball.Kind == FreezeBallKind.Mega ? FreezeRules.TileMegaSnowball : FreezeRules.TileSnowball;
                        QueueFreeze(tile.Id, FreezeRules.Shown(tile.TileState), save: false);
                    }
                }
            }
        }
    }

    private void ExplodeUnlocked(List<Snowball> due, IReadOnlyDictionary<long, (int X, int Y)> positions, List<FreezeActorUpdate> updates)
    {
        var tiles = new Dictionary<(int X, int Y), RoomFloorItem>();
        var blocks = new Dictionary<(int X, int Y), RoomFloorItem>();
        foreach (RoomFloorItem item in _floorItems.Values)
        {
            if (item.IsFreezeTile && (!tiles.TryGetValue((item.X, item.Y), out RoomFloorItem? known) || item.Z > known.Z))
            {
                tiles[(item.X, item.Y)] = item;
            }
            else if (item.IsFreezeBlock)
            {
                blocks[(item.X, item.Y)] = item;
            }
        }

        var targets = new Dictionary<(int X, int Y), List<FreezePlayer>>();
        foreach (FreezePlayer player in _freezePlayers.Values)
        {
            if (player.FrozenTicks == 0 && player.ShieldTicks == 0 && positions.TryGetValue(player.SessionId, out (int X, int Y) at))
            {
                (targets.TryGetValue(at, out List<FreezePlayer>? there) ? there : targets[at] = []).Add(player);
            }
        }

        var hit = new Dictionary<int, int>();
        var broken = new Dictionary<int, int>();
        var frozen = new Dictionary<long, Snowball>();
        var exploded = new HashSet<Snowball>();
        var pending = new Queue<(Snowball Ball, int ReachedAt)>(due.Select(ball => (ball, 0)));

        void Hit((int X, int Y) at, int step)
        {
            int id = tiles[at].Id;
            hit[id] = hit.TryGetValue(id, out int known) ? Math.Min(known, step) : step;
        }

        bool Freeze((int X, int Y) at, Snowball ball)
        {
            if (!targets.TryGetValue(at, out List<FreezePlayer>? there))
            {
                return false;
            }

            foreach (FreezePlayer player in there)
            {
                frozen.TryAdd(player.SessionId, ball);
            }

            return true;
        }

        while (pending.Count > 0)
        {
            (Snowball ball, int reachedAt) = pending.Dequeue();
            if (!exploded.Add(ball))
            {
                continue;
            }

            _snowballs.Remove(ball);
            if (_freezePlayers.TryGetValue(ball.Owner, out FreezePlayer? owner))
            {
                owner.BallsInAir = Math.Max(0, owner.BallsInAir - 1);
                owner.ReadyAtTick = _freezeTick + FreezeRules.ThrowCooldownTicks;
            }

            (int X, int Y) centre = (ball.X, ball.Y);
            if (!tiles.ContainsKey(centre))
            {
                continue;
            }

            Hit(centre, FreezeRules.DelayStep(0, reachedAt));
            Freeze(centre, ball);

            int reach = ball.Kind == FreezeBallKind.Mega ? FreezeRules.MegaReach : ball.Reach;
            foreach ((int dx, int dy) in FreezeRules.Directions(ball.Kind))
            {
                for (int distance = 1; distance <= reach; distance++)
                {
                    (int X, int Y) at = (ball.X + dx * distance, ball.Y + dy * distance);
                    if (!tiles.TryGetValue(at, out RoomFloorItem? tile))
                    {
                        break;
                    }

                    int step = FreezeRules.DelayStep(distance, reachedAt);
                    Hit(at, step);

                    if (blocks.TryGetValue(at, out RoomFloorItem? block) && FreezeRules.IsWhole(block.TileState))
                    {
                        block.TileState = FreezeRules.BrokenContent(FreezeRoll(FreezeRules.RollRange));
                        broken[block.Id] = step;
                        break;
                    }

                    if (_snowballs.FirstOrDefault(other => other.Shown && other.TileId == tile.Id && !exploded.Contains(other)) is { } reached)
                    {
                        pending.Enqueue((reached, step));
                    }

                    if (Freeze(at, ball))
                    {
                        break;
                    }
                }
            }
        }

        foreach ((int tileId, int step) in hit)
        {
            if (_floorItems.TryGetValue(tileId, out RoomFloorItem? tile))
            {
                tile.TileState = FreezeRules.TilePlain;
                QueueFreeze(tileId, FreezeRules.Shown(FreezeRules.TileBlast, step), save: false);
            }
        }

        foreach ((int blockId, int step) in broken)
        {
            QueueFreeze(blockId, FreezeRules.Shown(_floorItems[blockId].TileState, step), save: false);
        }

        foreach ((long sessionId, Snowball ball) in frozen)
        {
            FreezePlayer victim = _freezePlayers[sessionId];
            victim.FrozenTicks = FreezeRules.FrozenTicks;
            victim.ShieldTicks = 0;
            updates.Add(new FreezeActorUpdate(sessionId, FreezeRules.FrozenEffect, Frozen: true, EffectDelayMs: FreezeRules.FrozenEffectDelayMs));

            if (_freezeWithLives)
            {
                victim.Lives = Math.Max(0, victim.Lives - 1);
                _freezeLives.Add((victim.VirtualId, victim.Lives));
            }

            if (ball.OwnerTeam != victim.Team)
            {
                _freezeFreezes[ball.OwnerAccount] = _freezeFreezes.GetValueOrDefault(ball.OwnerAccount) + 1;
                if (!_freezeWithLives && ball.OwnerTeam != GameTeam.None)
                {
                    _freezeScores[ball.OwnerTeam] += FreezeRules.PointsPerFreeze;
                }
            }
        }

        if (frozen.Count > 0)
        {
            RecountFreezeScoresUnlocked();
        }
    }

    private bool IsLastTeamStandingUnlocked()
    {
        if (!_freezeWithLives || _freezeTeamsAtStart == 0)
        {
            return false;
        }

        int standing = FreezeRules.Teams.Count(team => _freezeTeams[team].Count > 0);
        return _freezeTeamsAtStart > 1 ? standing <= 1 : standing == 0;
    }

    private void EndFreezeUnlocked(bool ceremony, List<FreezeActorUpdate> updates)
    {
        _freezePhase = FreezePhase.Over;
        _freezeInProgress = false;
        _freezeGatesShut = false;

        lock (_furnitureLock)
        {
            foreach (Snowball ball in _snowballs)
            {
                if (ball.Shown && _floorItems.TryGetValue(ball.TileId, out RoomFloorItem? tile))
                {
                    tile.TileState = FreezeRules.TilePlain;
                    QueueFreeze(tile.Id, FreezeRules.Shown(FreezeRules.TilePlain), save: false);
                }
            }

            foreach (RoomFloorItem item in _floorItems.Values)
            {
                if (item.IsFreezeBlock && (item.TileState == FreezeRules.BlockEmpty || FreezeRules.IsPowerUp(item.TileState)))
                {
                    item.TileState = FreezeRules.Taken(item.TileState);
                    QueueFreeze(item.Id, FreezeRules.Shown(item.TileState), save: false);
                }
                else if (item.IsFreezeExit && item.TileState != FreezeRules.ExitOff)
                {
                    item.TileState = FreezeRules.ExitOff;
                    QueueFreeze(item.Id, FreezeRules.ExitOff.ToString(System.Globalization.CultureInfo.InvariantCulture), save: false);
                }
            }
        }

        _snowballs.Clear();
        foreach (FreezePlayer player in _freezePlayers.Values)
        {
            if (player.FrozenTicks > 0 || player.ShieldTicks > 0)
            {
                updates.Add(new FreezeActorUpdate(player.SessionId, FreezeRules.EffectOf(player.Team), Frozen: false));
            }

            player.FrozenTicks = 0;
            player.ShieldTicks = 0;
            player.BallsInAir = 0;
        }

        HashSet<GameTeam>? winners = ceremony ? FreezeRules.Winners(_freezeScores).ToHashSet() : null;
        if (winners is not null)
        {
            foreach (FreezePlayer player in _freezePlayers.Values.Where(player => winners.Contains(player.Team)))
            {
                _freezeWaves.Add(player.VirtualId);
            }
        }

        RecordFreezeGameUnlocked(winners);
    }

    private void RecordFreezeGameUnlocked(HashSet<GameTeam>? winners)
    {
        var players = new List<FreezePlayerResult>();
        if (winners is not null)
        {
            foreach ((long sessionId, (int accountId, GameTeam team)) in _freezeParticipants)
            {
                if (_actors.ContainsKey(sessionId))
                {
                    players.Add(new FreezePlayerResult(accountId, team, Math.Max(0, _freezeScores[team]), winners.Contains(team)));
                }
            }
        }

        if (players.Count > 0 || _freezeFreezes.Count > 0)
        {
            _freezeGames.Add(new FreezeGameResult(new Dictionary<int, int>(_freezeFreezes), players));
        }

        _freezeFreezes.Clear();
        _freezeParticipants.Clear();
        _freezeInProgress = false;
    }

    private void RecountFreezeScoresUnlocked(bool force = false)
    {
        foreach (GameTeam team in FreezeRules.Teams)
        {
            int score = _freezeWithLives
                ? _freezeTeams[team].Sum(sessionId => _freezePlayers.TryGetValue(sessionId, out FreezePlayer? player) ? player.Lives : 0) * FreezeRules.PointsPerLife
                : Math.Max(0, _freezeScores[team]);
            _freezeScores[team] = score;
            lock (_furnitureLock)
            {
                foreach (RoomFloorItem board in _floorItems.Values)
                {
                    if (board.IsFreezeScore && GameTeams.OfClassname(board.Classname) == team && (board.TileState != score || force))
                    {
                        board.TileState = score;
                        QueueFreeze(board.Id, score.ToString(System.Globalization.CultureInfo.InvariantCulture), save: true);
                    }
                }
            }
        }
    }

    private FreezeTickResult HandOutFreezeUnlocked()
    {
        bool busy = _freezePhase is FreezePhase.Running or FreezePhase.Ending || _freezeExits.Count > 0;
        if (_freezeChanges.Count == 0 && _freezePlaying.Count == 0 && _freezeLives.Count == 0 && _freezeWaves.Count == 0
            && _freezePowerUps.Count == 0 && _freezeStopClock is null && _freezeGames.Count == 0)
        {
            return busy ? new FreezeTickResult([], [], [], [], [], null, [], true) : FreezeTickResult.Idle;
        }

        var result = new FreezeTickResult([.. _freezeChanges], [.. _freezePlaying], [.. _freezeLives], [.. _freezeWaves], [.. _freezePowerUps],
            _freezeStopClock, [.. _freezeGames], busy);
        _freezeChanges.Clear();
        _freezePlaying.Clear();
        _freezeLives.Clear();
        _freezeWaves.Clear();
        _freezePowerUps.Clear();
        _freezeGames.Clear();
        _freezeStopClock = null;
        return result;
    }

    private void ApplyFreezeUpdates(List<FreezeActorUpdate> updates, List<RoomActor>? changed)
    {
        foreach (FreezeActorUpdate update in updates)
        {
            if (!_actors.TryGetValue(update.SessionId, out RoomActor? actor))
            {
                continue;
            }

            lock (actor.Lock)
            {
                if (update.OnlyOutOfTeam && actor.Team != GameTeam.None)
                {
                    continue;
                }

                if (update.LeaveTeam)
                {
                    actor.Team = GameTeam.None;
                    actor.InFreeze = false;
                }

                if (update.Frozen is { } frozen)
                {
                    actor.FrozenInGame = frozen;
                    if (frozen)
                    {
                        actor.IsWalking = false;
                        actor.Path = null;
                        actor.Goal = null;
                    }
                }

                if (update.Effect is { } effect && (actor.InFreeze || update.LeaveTeam || update.OnlyOutOfTeam))
                {
                    actor.EffectDelayMs = update.EffectDelayMs;
                    SetGameEffect(actor, effect);
                }

                if (update.TeleportTo is { } to && actor.Team == GameTeam.None)
                {
                    actor.X = to.X;
                    actor.Y = to.Y;
                    actor.Z = SurfaceHeightAt(to.X, to.Y);
                    actor.PendingStep = null;
                    actor.IsWalking = false;
                    actor.Path = null;
                    actor.Goal = null;
                    ApplyTilePosture(actor);
                    UpdateWaterEffect(actor);
                    if (changed is not null && !changed.Contains(actor))
                    {
                        changed.Add(actor);
                    }
                }
            }
        }
    }

    private RoomFloorItem? FreezeTileAtUnlocked(int x, int y) => ItemsAtUnlocked(x, y).LastOrDefault(item => item.IsFreezeTile);

    private RoomFloorItem? FreezeBlockAtUnlocked(int x, int y) => ItemsAtUnlocked(x, y).LastOrDefault(item => item.IsFreezeBlock);

    private readonly record struct FreezeActorUpdate(
        long SessionId, int? Effect = null, bool? Frozen = null, bool LeaveTeam = false, (int X, int Y)? TeleportTo = null,
        int EffectDelayMs = 0, bool OnlyOutOfTeam = false);

    private sealed class FreezeExitTrip(long sessionId, int ticksToExit, int ticksToClear)
    {
        public long SessionId { get; } = sessionId;

        public int TicksToExit { get; set; } = ticksToExit;

        public int TicksToClear { get; set; } = ticksToClear;
    }

    private sealed class FreezePlayer(long sessionId, int accountId, int virtualId, GameTeam team)
    {
        public long SessionId { get; } = sessionId;

        public int AccountId { get; } = accountId;

        public int VirtualId { get; } = virtualId;

        public GameTeam Team { get; } = team;

        public int Lives { get; set; }

        public int Reach { get; set; } = FreezeRules.StartReach;

        public int MaxBalls { get; set; } = 1;

        public int BallsInAir { get; set; }

        public FreezeBallKind NextBall { get; set; }

        public int FrozenTicks { get; set; }

        public int ShieldTicks { get; set; }

        public int ReadyAtTick { get; set; }

        public void Reset(int lives)
        {
            Lives = lives;
            Reach = FreezeRules.StartReach;
            MaxBalls = 1;
            BallsInAir = 0;
            NextBall = FreezeBallKind.Normal;
            FrozenTicks = 0;
            ShieldTicks = 0;
            ReadyAtTick = 0;
        }
    }

    private sealed class Snowball(int tileId, int x, int y, long owner, int ownerAccount, GameTeam ownerTeam, FreezeBallKind kind, int reach)
    {
        public int TileId { get; } = tileId;

        public int X { get; } = x;

        public int Y { get; } = y;

        public long Owner { get; set; } = owner;

        public int OwnerAccount { get; } = ownerAccount;

        public GameTeam OwnerTeam { get; } = ownerTeam;

        public FreezeBallKind Kind { get; } = kind;

        public int Reach { get; } = reach;

        public bool Shown { get; set; }

        public int FuseLeft { get; set; } = FreezeRules.FuseTicks;
    }
}
