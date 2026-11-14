using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public readonly record struct BanzaiChange(int ItemId, string Data, bool Save);

public sealed record BanzaiTickResult(IReadOnlyList<BanzaiChange> Changes, bool Busy)
{
    public static BanzaiTickResult Idle { get; } = new([], false);
}

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

    private readonly List<(int X, int Y, GameTeam Team)> _puckPaints = [];

    private BanzaiPhase _banzaiPhase;
    private int _banzaiClockId;
    private int _banzaiTicksLeft;
    private volatile bool _banzaiInProgress;

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

    public bool BanzaiBusy
    {
        get
        {
            lock (_banzaiLock)
            {
                return _banzaiPhase is BanzaiPhase.Ending or BanzaiPhase.Blinking || _banzaiChanges.Count > 0;
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
        if (!_banzaiInProgress)
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
            _banzaiPhase = BanzaiPhase.Running;
            _banzaiClockId = clockId;
            _banzaiBlink.Clear();
            _banzaiInProgress = true;

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
                        Queue(item.Id, 0, save: true);
                    }
                }
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
            return true;
        }
    }

    private void PlayBanzai(RoomActor actor, int x, int y)
    {
        if (actor.Type != ActorType.Player)
        {
            return;
        }

        RoomFloorItem? top;
        lock (_furnitureLock)
        {
            _topItemByTile.TryGetValue((x, y), out top);
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
                    PaintUnlocked(x, y, actor.Team);
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

    private void PaintUnlocked(int x, int y, GameTeam team)
    {
        lock (_furnitureLock)
        {
            if (!_topItemByTile.TryGetValue((x, y), out RoomFloorItem? tile) || !tile.IsBanzaiTile)
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
                FillEnclosedUnlocked(team);
                RecountScoresUnlocked();
            }
        }
    }

    private void FillEnclosedUnlocked(GameTeam team)
    {
        var tiles = new Dictionary<(int X, int Y), RoomFloorItem>();
        foreach (RoomFloorItem item in _floorItems.Values)
        {
            if (item.IsBanzaiTile)
            {
                tiles.TryAdd((item.X, item.Y), item);
            }
        }

        int lockedState = BanzaiRules.LockedState(team);
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

            foreach ((int X, int Y) position in region)
            {
                RoomFloorItem tile = tiles[position];
                tile.TileState = lockedState;
                Queue(tile.Id, lockedState, save: true);
            }
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
            if (counts[team] == _banzaiScores[team])
            {
                continue;
            }

            _banzaiScores[team] = counts[team];
            foreach (RoomFloorItem board in _floorItems.Values)
            {
                if (board.IsBanzaiScore && GameTeams.OfClassname(board.Classname) == team)
                {
                    Queue(board.Id, counts[team], save: true);
                }
            }
        }
    }

    private void Queue(int itemId, int state, bool save) =>
        _banzaiChanges.Add(new BanzaiChange(itemId, state.ToString(System.Globalization.CultureInfo.InvariantCulture), save));

    public BanzaiTickResult AdvanceBanzai(IReadOnlyList<CountdownFinished> finishedClocks)
    {
        if (!_hasBanzai && !_banzaiInProgress && (_banzaiPhase is BanzaiPhase.Idle or BanzaiPhase.Over))
        {
            return BanzaiTickResult.Idle;
        }

        List<(int X, int Y, GameTeam Team)> paints;
        lock (_footballLock)
        {
            paints = _puckPaints.Count == 0 ? [] : [.. _puckPaints];
            _puckPaints.Clear();
        }

        lock (_banzaiLock)
        {
            switch (_banzaiPhase)
            {
                case BanzaiPhase.Running:
                    if (GetFloorItem(_banzaiClockId) is null)
                    {
                        EndBanzaiUnlocked(blink: false);
                    }
                    else if (finishedClocks.Any(clock => clock.ItemId == _banzaiClockId))
                    {
                        _banzaiPhase = BanzaiPhase.Ending;
                        _banzaiTicksLeft = BanzaiRules.EndDelayTicks;
                    }
                    else
                    {
                        foreach ((int x, int y, GameTeam team) in paints)
                        {
                            PaintUnlocked(x, y, team);
                        }
                    }

                    break;

                case BanzaiPhase.Ending:
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

            if (_banzaiChanges.Count == 0)
            {
                return new BanzaiTickResult([], _banzaiPhase is BanzaiPhase.Ending or BanzaiPhase.Blinking);
            }

            BanzaiChange[] changes = [.. _banzaiChanges];
            _banzaiChanges.Clear();
            return new BanzaiTickResult(changes, _banzaiPhase is BanzaiPhase.Ending or BanzaiPhase.Blinking);
        }
    }

    private void EndBanzaiUnlocked(bool blink)
    {
        _banzaiInProgress = false;
        _banzaiBlink.Clear();

        lock (_furnitureLock)
        {
            RecountScoresUnlocked();
            int best = _banzaiScores.Values.Max();
            var winners = new HashSet<GameTeam>(BanzaiRules.Teams.Where(team => best > 0 && _banzaiScores[team] == best));

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
