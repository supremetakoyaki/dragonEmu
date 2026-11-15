using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public readonly record struct BanzaiChange(int ItemId, string Data, bool Save, bool Alone = false);

public sealed record BanzaiTickResult(IReadOnlyList<BanzaiChange> Changes, bool Busy, IReadOnlyList<BanzaiGameResult>? Games = null)
{
    public static BanzaiTickResult Idle { get; } = new([], false);
}

public sealed record BanzaiGameResult(IReadOnlyDictionary<int, int> TilesLocked, IReadOnlyList<BanzaiPlayerResult> Players);

public readonly record struct BanzaiPlayerResult(int AccountId, GameTeam Team, int TeamScore, bool Won);

public enum BanzaiPhase
{
    Idle,

    Running,

    Paused,

    Ending,

    Blinking,

    Over,
}

public sealed partial class Room
{
    private readonly object _banzaiLock = new();
    private readonly Dictionary<GameTeam, HashSet<long>> _banzaiTeams = BanzaiRules.Teams.ToDictionary(team => team, _ => new HashSet<long>());
    private readonly Dictionary<GameTeam, int> _banzaiScores = BanzaiRules.Teams.ToDictionary(team => team, _ => 0);
    private readonly List<BanzaiChange> _banzaiChanges = [];
    private readonly List<(int ItemId, int LockedState)> _banzaiBlink = [];

    private readonly Dictionary<int, int> _banzaiTilesLocked = [];
    private readonly List<BanzaiGameResult> _banzaiGames = [];

    private readonly List<(int X, int Y, GameTeam Team, int? AccountId, long DueAtMs)> _puckPaints = [];

    private BanzaiPhase _banzaiPhase;
    private int _banzaiClockId;
    private int _banzaiTicksLeft;
    private volatile bool _banzaiInProgress;

    private volatile bool _banzaiGatesShut;

    private volatile bool _banzaiResync;
    private long _banzaiItemsSignature;
    private bool _banzaiItemsNoted;

    private volatile bool _hasBanzai;

    public BanzaiPhase BanzaiPhase
    {
        get
        {
            lock (_banzaiLock)
            {
                return _banzaiPhase;
            }
        }
    }

    public bool BanzaiInProgress => _banzaiInProgress;

    public bool BanzaiGatesShut => _banzaiGatesShut;

    public bool BanzaiBusy
    {
        get
        {
            lock (_banzaiLock)
            {
                if (_banzaiPhase is BanzaiPhase.Ending or BanzaiPhase.Blinking || _banzaiChanges.Count > 0 || _banzaiResync)
                {
                    return true;
                }
            }

            lock (_footballLock)
            {
                return _puckPaints.Count > 0;
            }
        }
    }

    public int BanzaiPlayers(GameTeam team)
    {
        lock (_banzaiLock)
        {
            return _banzaiTeams.TryGetValue(team, out HashSet<long>? players) ? players.Count : 0;
        }
    }

    public int BanzaiScore(GameTeam team)
    {
        lock (_banzaiLock)
        {
            return _banzaiScores.GetValueOrDefault(team);
        }
    }

    private bool IsShutBanzaiGate(int x, int y)
    {
        if (!_banzaiGatesShut)
        {
            return false;
        }

        lock (_furnitureLock)
        {
            return ItemsAtUnlocked(x, y).Any(item => item.IsBanzaiGate);
        }
    }

    private HashSet<(int X, int Y)> BanzaiGateTiles()
    {
        var tiles = new HashSet<(int X, int Y)>();
        lock (_furnitureLock)
        {
            foreach (RoomFloorItem gate in _floorItems.Values)
            {
                if (gate.IsBanzaiGate)
                {
                    tiles.UnionWith(gate.OccupiedTiles());
                }
            }
        }

        return tiles;
    }

    public void StartBanzai(int clockId)
    {
        lock (_banzaiLock)
        {
            if (_banzaiTilesLocked.Count > 0)
            {
                RecordGameUnlocked(winners: null);
            }

            _banzaiPhase = BanzaiPhase.Running;
            _banzaiClockId = clockId;
            _banzaiBlink.Clear();
            _banzaiInProgress = true;
            _banzaiGatesShut = true;

            lock (_furnitureLock)
            {
                foreach (RoomFloorItem item in _floorItems.Values)
                {
                    if (item.IsBanzaiTile)
                    {
                        item.TileState = BanzaiRules.TileLit;
                        Queue(item.Id, BanzaiRules.TileLit, save: true);
                    }
                    else if (item.IsBanzaiScore)
                    {
                        item.TileState = 0;
                        Queue(item.Id, 0, save: true);
                    }
                }

                ShowOnApparatusUnlocked(ApparatusPlaying, restart: true);
            }

            foreach (GameTeam team in BanzaiRules.Teams)
            {
                _banzaiScores[team] = 0;
            }
        }
    }

    public bool PauseBanzai()
    {
        lock (_banzaiLock)
        {
            if (_banzaiPhase != BanzaiPhase.Running)
            {
                return false;
            }

            _banzaiPhase = BanzaiPhase.Paused;
            _banzaiGatesShut = false;
            return true;
        }
    }

    public bool ResumeBanzai(int clockId)
    {
        lock (_banzaiLock)
        {
            if (_banzaiPhase != BanzaiPhase.Paused)
            {
                return false;
            }

            _banzaiPhase = BanzaiPhase.Running;
            _banzaiClockId = clockId;
            _banzaiGatesShut = true;
            return true;
        }
    }

    private void PlayBanzai(RoomActor actor, int x, int y)
    {
        if (actor.Type != ActorType.Player || !_hasBanzai)
        {
            return;
        }

        RoomFloorItem? top;
        lock (_furnitureLock)
        {
            top = TopUnderBallsUnlocked(x, y);
        }

        if (top is null)
        {
            return;
        }

        if (top.IsBanzaiGate)
        {
            UseBanzaiGate(actor, GameTeams.OfClassname(top.Classname));
        }
        else if (top.IsBanzaiTile && actor.Team != GameTeam.None)
        {
            lock (_banzaiLock)
            {
                if (_banzaiPhase == BanzaiPhase.Running)
                {
                    PaintUnlocked(x, y, actor.Team, actor.AccountId);
                }
            }
        }
    }

    private void UseBanzaiGate(RoomActor actor, GameTeam team)
    {
        if (team == GameTeam.None)
        {
            return;
        }

        lock (_banzaiLock)
        {
            if (actor.Team == team)
            {
                _banzaiTeams[team].Remove(actor.SessionId);
                actor.Team = GameTeam.None;
                SetGameEffect(actor, 0);
                QueueGates(team);
                return;
            }

            if (_banzaiTeams[team].Count >= BanzaiRules.MaxPlayersPerTeam)
            {
                return;
            }

            if (actor.Team != GameTeam.None)
            {
                _banzaiTeams[actor.Team].Remove(actor.SessionId);
                QueueGates(actor.Team);
            }

            _banzaiTeams[team].Add(actor.SessionId);
            actor.Team = team;
            SetGameEffect(actor, BanzaiRules.EffectOf(team));
            QueueGates(team);
        }
    }

    private void LeaveBanzaiTeam(RoomActor actor)
    {
        GameTeam team;
        lock (actor.Lock)
        {
            team = actor.Team;
            actor.Team = GameTeam.None;
        }

        if (team == GameTeam.None)
        {
            return;
        }

        lock (_banzaiLock)
        {
            if (_banzaiTeams[team].Remove(actor.SessionId))
            {
                QueueGates(team);
            }
        }
    }

    private void SetGameEffect(RoomActor actor, int effectId)
    {
        if (actor.WaterEffectId > 0)
        {
            actor.EffectBeforeWater = effectId;
            return;
        }

        actor.EffectId = effectId;
        QueueEffectChange(actor);
    }

    private void QueueGates(GameTeam team)
    {
        int count = _banzaiTeams[team].Count;
        lock (_furnitureLock)
        {
            foreach (RoomFloorItem gate in _floorItems.Values)
            {
                if (gate.IsBanzaiGate && GameTeams.OfClassname(gate.Classname) == team)
                {
                    Queue(gate.Id, count, save: true);
                }
            }
        }
    }

    private void PaintUnlocked(int x, int y, GameTeam team, int? accountId = null)
    {
        lock (_furnitureLock)
        {
            if (TopUnderBallsUnlocked(x, y) is not { IsBanzaiTile: true } tile)
            {
                return;
            }

            int state = BanzaiRules.Paint(tile.TileState, team);
            if (state == tile.TileState)
            {
                return;
            }

            tile.TileState = state;
            Queue(tile.Id, state, save: true);

            if (BanzaiRules.IsLocked(state))
            {
                int locked = 1 + FillEnclosedUnlocked(team);
                RecountScoresUnlocked();
                if (accountId is { } account)
                {
                    _banzaiTilesLocked[account] = _banzaiTilesLocked.GetValueOrDefault(account) + locked;
                }
            }
        }
    }

    private int FillEnclosedUnlocked(GameTeam team)
    {
        var tiles = new Dictionary<(int X, int Y), RoomFloorItem>();
        var under = new List<RoomFloorItem>();
        foreach (RoomFloorItem item in _floorItems.Values)
        {
            if (!item.IsBanzaiTile)
            {
                continue;
            }

            if (!tiles.TryGetValue((item.X, item.Y), out RoomFloorItem? known))
            {
                tiles[(item.X, item.Y)] = item;
            }
            else if (item.Z > known.Z)
            {
                tiles[(item.X, item.Y)] = item;
                under.Add(known);
            }
            else
            {
                under.Add(item);
            }
        }

        int lockedState = BanzaiRules.LockedState(team);
        int filled = 0;
        var seen = new HashSet<(int X, int Y)>();
        foreach ((int X, int Y) start in tiles.Keys)
        {
            if (tiles[start].TileState == lockedState || !seen.Add(start))
            {
                continue;
            }

            var region = new List<(int X, int Y)> { start };
            var frontier = new Queue<(int X, int Y)>();
            frontier.Enqueue(start);
            bool shutIn = true;
            while (frontier.Count > 0)
            {
                (int X, int Y) at = frontier.Dequeue();
                foreach ((int sx, int sy) in Sides)
                {
                    (int X, int Y) next = (at.X + sx, at.Y + sy);
                    if (!tiles.TryGetValue(next, out RoomFloorItem? tile))
                    {
                        shutIn = false;
                    }
                    else if (tile.TileState != lockedState && seen.Add(next))
                    {
                        region.Add(next);
                        frontier.Enqueue(next);
                    }
                }
            }

            if (!shutIn)
            {
                continue;
            }

            filled += region.Count;
            foreach ((int X, int Y) position in region)
            {
                RoomFloorItem tile = tiles[position];
                tile.TileState = lockedState;
                Queue(tile.Id, lockedState, save: true);
            }
        }

        foreach (RoomFloorItem tile in under)
        {
            if (tiles.TryGetValue((tile.X, tile.Y), out RoomFloorItem? top) && tile.TileState != top.TileState)
            {
                tile.TileState = top.TileState;
                Queue(tile.Id, top.TileState, save: true);
            }
        }

        return filled;
    }

    private RoomFloorItem? TopUnderBallsUnlocked(int x, int y)
    {
        IReadOnlyList<RoomFloorItem> items = ItemsAtUnlocked(x, y);
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (!items[i].IsBall && !items[i].IsBanzaiPyramid)
            {
                return items[i];
            }
        }

        return null;
    }

    private void NoteBanzaiItems()
    {
        long signature = 0;
        int count = 0;
        foreach (RoomFloorItem item in _floorItems.Values)
        {
            if (item.IsBanzaiTile || item.IsBanzaiScore)
            {
                signature += item.Id * 31L + 7;
                count++;
            }
        }

        signature ^= (long)count << 48;
        if (!_banzaiItemsNoted)
        {
            _banzaiItemsNoted = true;
            _banzaiItemsSignature = signature;
        }
        else if (signature != _banzaiItemsSignature)
        {
            _banzaiItemsSignature = signature;
            _banzaiResync = true;
        }
    }

    private static readonly (int X, int Y)[] Sides = [(0, -1), (1, 0), (0, 1), (-1, 0)];

    private void RecountScoresUnlocked()
    {
        var counts = BanzaiRules.Teams.ToDictionary(team => team, _ => 0);
        foreach (RoomFloorItem item in _floorItems.Values)
        {
            if (item.IsBanzaiTile && BanzaiRules.IsLocked(item.TileState))
            {
                counts[BanzaiRules.TeamOfTile(item.TileState)]++;
            }
        }

        foreach (GameTeam team in BanzaiRules.Teams)
        {
            _banzaiScores[team] = counts[team];
        }

        foreach (RoomFloorItem board in _floorItems.Values)
        {
            if (board.IsBanzaiScore && counts.TryGetValue(GameTeams.OfClassname(board.Classname), out int count) && board.TileState != count)
            {
                board.TileState = count;
                Queue(board.Id, count, save: true);
            }
        }

        if (_banzaiPhase == BanzaiPhase.Running)
        {
            ShowOnApparatusUnlocked(ApparatusLeaderState());
        }
    }

    private void Queue(int itemId, int state, bool save) =>
        _banzaiChanges.Add(new BanzaiChange(itemId, state.ToString(System.Globalization.CultureInfo.InvariantCulture), save));

    public BanzaiTickResult AdvanceBanzai(IReadOnlyList<CountdownFinished> finishedClocks) => AdvanceBanzai(finishedClocks, long.MaxValue);

    public BanzaiTickResult AdvanceBanzai(IReadOnlyList<CountdownFinished> finishedClocks, long nowMs)
    {
        if (!_hasBanzai && !_banzaiInProgress && !_banzaiResync && (_banzaiPhase is BanzaiPhase.Idle or BanzaiPhase.Over))
        {
            return BanzaiTickResult.Idle;
        }

        List<(int X, int Y, GameTeam Team, int? AccountId)> paints = TakeDuePuckPaints(nowMs);

        lock (_banzaiLock)
        {
            if (_banzaiResync)
            {
                _banzaiResync = false;
                lock (_furnitureLock)
                {
                    RecountScoresUnlocked();
                }
            }

            switch (_banzaiPhase)
            {
                case BanzaiPhase.Running:
                    if (GetFloorItem(_banzaiClockId) is null)
                    {
                        EndBanzaiUnlocked(blink: false);
                    }
                    else
                    {
                        foreach ((int x, int y, GameTeam team, int? accountId) in paints)
                        {
                            PaintUnlocked(x, y, team, accountId);
                        }

                        if (finishedClocks.Any(clock => clock.ItemId == _banzaiClockId))
                        {
                            _banzaiPhase = BanzaiPhase.Ending;
                            _banzaiTicksLeft = BanzaiRules.EndDelayTicks;
                        }
                    }

                    break;

                case BanzaiPhase.Ending:
                    if (_banzaiTicksLeft == BanzaiRules.EndDelayTicks)
                    {
                        lock (_furnitureLock)
                        {
                            ShowOnApparatusUnlocked(ApparatusResultState());
                        }
                    }

                    if (--_banzaiTicksLeft <= 0)
                    {
                        EndBanzaiUnlocked(blink: true);
                    }

                    break;

                case BanzaiPhase.Blinking:
                    int frame = BanzaiRules.BlinkFrames - _banzaiTicksLeft;
                    _banzaiTicksLeft--;
                    bool last = _banzaiTicksLeft == 0;
                    foreach ((int itemId, int lockedState) in _banzaiBlink)
                    {
                        Queue(itemId, BanzaiRules.BlinkFrame(frame, lockedState), save: last);
                    }

                    if (last)
                    {
                        _banzaiBlink.Clear();
                        _banzaiPhase = BanzaiPhase.Over;
                    }

                    break;
            }

            return HandOutUnlocked(_banzaiPhase is BanzaiPhase.Ending or BanzaiPhase.Blinking || PuckPaintsPending());
        }
    }

    public BanzaiTickResult AdvancePuckPaints(long nowMs)
    {
        if (!_hasBanzai)
        {
            return BanzaiTickResult.Idle;
        }

        List<(int X, int Y, GameTeam Team, int? AccountId)> paints = TakeDuePuckPaints(nowMs);
        if (paints.Count == 0)
        {
            return BanzaiTickResult.Idle;
        }

        lock (_banzaiLock)
        {
            if (_banzaiPhase == BanzaiPhase.Running)
            {
                foreach ((int x, int y, GameTeam team, int? accountId) in paints)
                {
                    PaintUnlocked(x, y, team, accountId);
                }
            }

            return HandOutUnlocked(busy: false);
        }
    }

    private BanzaiTickResult HandOutUnlocked(bool busy)
    {
        BanzaiGameResult[]? games = null;
        if (_banzaiGames.Count > 0)
        {
            games = [.. _banzaiGames];
            _banzaiGames.Clear();
        }

        if (_banzaiChanges.Count == 0)
        {
            return games is null ? new BanzaiTickResult([], busy) : new BanzaiTickResult([], busy, games);
        }

        BanzaiChange[] changes = [.. _banzaiChanges];
        _banzaiChanges.Clear();
        return new BanzaiTickResult(changes, busy, games);
    }

    private void RecordGameUnlocked(HashSet<GameTeam>? winners)
    {
        var players = new List<BanzaiPlayerResult>();
        if (winners is not null)
        {
            foreach (GameTeam team in BanzaiRules.Teams)
            {
                foreach (long sessionId in _banzaiTeams[team])
                {
                    if (_actors.TryGetValue(sessionId, out RoomActor? actor) && actor.Type == ActorType.Player)
                    {
                        players.Add(new BanzaiPlayerResult(actor.AccountId, team, _banzaiScores[team], winners.Contains(team)));
                    }
                }
            }
        }

        if (players.Count > 0 || _banzaiTilesLocked.Count > 0)
        {
            _banzaiGames.Add(new BanzaiGameResult(new Dictionary<int, int>(_banzaiTilesLocked), players));
        }

        _banzaiTilesLocked.Clear();
    }

    private List<(int X, int Y, GameTeam Team, int? AccountId)> TakeDuePuckPaints(long nowMs)
    {
        lock (_footballLock)
        {
            if (_puckPaints.Count == 0)
            {
                return [];
            }

            var due = new List<(int X, int Y, GameTeam Team, int? AccountId)>();
            _puckPaints.RemoveAll(paint =>
            {
                if (paint.DueAtMs > nowMs)
                {
                    return false;
                }

                due.Add((paint.X, paint.Y, paint.Team, paint.AccountId));
                return true;
            });
            return due;
        }
    }

    private bool PuckPaintsPending()
    {
        lock (_footballLock)
        {
            return _puckPaints.Count > 0;
        }
    }

    private void QueuePuckPaint(RoomFloorItem ball, int x, int y, GameTeam team, long dueAtMs, int? accountId)
    {
        if (ball.IsBanzaiPuck && team != GameTeam.None)
        {
            _puckPaints.Add((x, y, team, accountId, dueAtMs));
        }
    }

    private void EndBanzaiUnlocked(bool blink)
    {
        _banzaiInProgress = false;
        _banzaiGatesShut = false;
        _banzaiBlink.Clear();

        lock (_furnitureLock)
        {
            RecountScoresUnlocked();
            int best = _banzaiScores.Values.Max();
            var winners = new HashSet<GameTeam>(BanzaiRules.Teams.Where(team => best > 0 && _banzaiScores[team] == best));
            RecordGameUnlocked(blink ? winners : null);
            ShowOnApparatusUnlocked(blink ? ApparatusResultState() : ApparatusOn);

            foreach (RoomFloorItem item in _floorItems.Values)
            {
                if (!item.IsBanzaiTile)
                {
                    continue;
                }

                if (item.TileState == BanzaiRules.TileLit)
                {
                    item.TileState = BanzaiRules.TileOff;
                    Queue(item.Id, BanzaiRules.TileOff, save: true);
                }
                else if (blink && BanzaiRules.IsLocked(item.TileState) && winners.Contains(BanzaiRules.TeamOfTile(item.TileState)))
                {
                    _banzaiBlink.Add((item.Id, item.TileState));
                }
            }
        }

        if (_banzaiBlink.Count > 0)
        {
            _banzaiPhase = BanzaiPhase.Blinking;
            _banzaiTicksLeft = BanzaiRules.BlinkFrames;
        }
        else
        {
            _banzaiPhase = BanzaiPhase.Over;
        }
    }
}
