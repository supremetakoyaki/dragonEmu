using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    public const int ApparatusOff = 0;
    public const int ApparatusPlaying = 1;
    public const int ApparatusOn = 2;
    public const int ApparatusTie = 3;

    public const int PyramidUp = 0;
    public const int PyramidDown = 1;
    public const int PyramidMinTicks = 12;
    public const int PyramidMaxTicks = 24;

    private readonly Dictionary<int, int> _pyramidDue = [];
    private int _pyramidTick;

    public int? ToggleApparatus(int itemId)
    {
        lock (_banzaiLock)
        {
            if (_banzaiInProgress || _banzaiPhase is BanzaiPhase.Ending or BanzaiPhase.Blinking)
            {
                return null;
            }

            lock (_furnitureLock)
            {
                if (!_floorItems.TryGetValue(itemId, out RoomFloorItem? apparatus) || !apparatus.IsBanzaiApparatus)
                {
                    return null;
                }

                apparatus.TileState = apparatus.TileState == ApparatusOff ? ApparatusOn : ApparatusOff;
                return apparatus.TileState;
            }
        }
    }

    private void ShowOnApparatusUnlocked(int state, bool restart = false)
    {
        foreach (RoomFloorItem apparatus in _floorItems.Values)
        {
            if (!apparatus.IsBanzaiApparatus || (apparatus.TileState == state && !restart))
            {
                continue;
            }

            if (restart)
            {
                _banzaiChanges.Add(new BanzaiChange(apparatus.Id, "0", Save: false, Alone: true));
            }

            apparatus.TileState = state;
            Queue(apparatus.Id, state, save: true);
        }
    }

    private int ApparatusLeaderState()
    {
        int best = _banzaiScores.Values.Max();
        GameTeam[] ahead = BanzaiRules.Teams.Where(team => best > 0 && _banzaiScores[team] == best).ToArray();
        return ahead.Length == 1 ? 9 + BanzaiRules.IndexOf(ahead[0]) : ApparatusPlaying;
    }

    private int ApparatusResultState()
    {
        int best = _banzaiScores.Values.Max();
        GameTeam[] winners = BanzaiRules.Teams.Where(team => best > 0 && _banzaiScores[team] == best).ToArray();
        return winners.Length == 1 ? 19 + BanzaiRules.IndexOf(winners[0]) : ApparatusTie;
    }

    public IReadOnlyList<FurnitureStateChange> AdvancePyramids(out bool busy)
    {
        lock (_furnitureLock)
        {
            busy = _floorItems.Values.Any(item => item.IsBanzaiPyramid);
            if (!busy)
            {
                _pyramidDue.Clear();
                return [];
            }
        }

        var occupied = new HashSet<(int X, int Y)>();
        foreach (RoomActor actor in _actors.Values)
        {
            lock (actor.Lock)
            {
                occupied.Add((actor.X, actor.Y));
                if (actor.PendingStep is { } step)
                {
                    occupied.Add((step.X, step.Y));
                }
            }
        }

        var changes = new List<FurnitureStateChange>();
        lock (_furnitureLock)
        {
            _pyramidTick++;
            var present = new HashSet<int>();
            foreach (RoomFloorItem pyramid in _floorItems.Values)
            {
                if (!pyramid.IsBanzaiPyramid)
                {
                    continue;
                }

                present.Add(pyramid.Id);
                if (!_pyramidDue.TryGetValue(pyramid.Id, out int due))
                {
                    _pyramidDue[pyramid.Id] = _pyramidTick + NextPyramidWait();
                    continue;
                }

                if (_pyramidTick < due)
                {
                    continue;
                }

                bool goingUp = pyramid.TileState == PyramidDown;
                if (goingUp && pyramid.OccupiedTiles().Any(occupied.Contains))
                {
                    continue;
                }

                pyramid.TileState = goingUp ? PyramidUp : PyramidDown;
                changes.Add(new FurnitureStateChange(pyramid.Id, pyramid.TileState.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                _pyramidDue[pyramid.Id] = _pyramidTick + NextPyramidWait();
            }

            foreach (int gone in _pyramidDue.Keys.Where(id => !present.Contains(id)).ToList())
            {
                _pyramidDue.Remove(gone);
            }
        }

        return changes;
    }

    private static int NextPyramidWait() => Random.Shared.Next(PyramidMinTicks, PyramidMaxTicks + 1);
}
