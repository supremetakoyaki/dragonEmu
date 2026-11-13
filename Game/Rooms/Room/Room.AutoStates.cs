using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public readonly record struct AutoStateChange(RoomFloorItem Item, int State, bool First);

public sealed partial class Room
{
    private readonly Dictionary<int, int> _autoStates = [];

    private volatile bool _autoRecheck;
    private volatile bool _hasAutoItems;

    public bool AutoStatesPending => _autoRecheck && _hasAutoItems;

    private void NoteAutoItems()
    {
        _hasAutoItems = _floorItems.Values.Any(item => item.IsAutoState);
        _autoRecheck = true;
    }

    private int InflatableState(RoomFloorItem inflatable)
    {
        foreach ((int x, int y) in inflatable.OccupiedTiles())
        {
            RoomFloorItem? under = null;
            if (_itemsByTile.TryGetValue((x, y), out List<RoomFloorItem>? items))
            {
                foreach (RoomFloorItem item in items)
                {
                    if (item.Id != inflatable.Id && item.SurfaceZ <= inflatable.Z + 0.001 && (under is null || item.SurfaceZ >= under.SurfaceZ))
                    {
                        under = item;
                    }
                }
            }

            if (under is not { IsWaterArea: true })
            {
                return 0;
            }
        }

        return 1;
    }

    public IReadOnlyList<AutoStateChange> AdvanceAutoStates() => AdvanceAutoStates([]);

    public IReadOnlyList<AutoStateChange> AdvanceAutoStates(List<RoomActor> changed)
    {
        _autoRecheck = false;
        if (!_hasAutoItems)
        {
            return [];
        }

        var standing = new HashSet<(int X, int Y)>();
        foreach (RoomActor actor in _actors.Values)
        {
            lock (actor.Lock)
            {
                standing.Add((actor.X, actor.Y));
            }
        }

        var changes = new List<AutoStateChange>();
        var movedTiles = new HashSet<(int X, int Y)>();
        lock (_furnitureLock)
        {
            foreach (RoomFloorItem item in _floorItems.Values)
            {
                int state = item.InteractionLogic switch
                {
                    FurnitureInteractionLogic.PoolInflatable => InflatableState(item),
                    FurnitureInteractionLogic.PressurePlate => item.OccupiedTiles().Any(standing.Contains) ? 1 : 0,
                    _ => -1,
                };

                if (state < 0)
                {
                    continue;
                }

                bool known = _autoStates.TryGetValue(item.Id, out int before);
                if (!known || before != state)
                {
                    _autoStates[item.Id] = state;
                    changes.Add(new AutoStateChange(item, state, First: !known));

                    if (item.InteractionLogic == FurnitureInteractionLogic.PressurePlate && item.Pressed != (state == 1))
                    {
                        item.Pressed = state == 1;
                        movedTiles.UnionWith(item.OccupiedTiles());
                    }
                }
            }

            if (movedTiles.Count > 0)
            {
                RebuildTileIndex();
            }
        }

        if (movedTiles.Count > 0)
        {
            foreach (long sessionId in ActorsOn(movedTiles))
            {
                if (!_actors.TryGetValue(sessionId, out RoomActor? actor))
                {
                    continue;
                }

                lock (actor.Lock)
                {
                    if (actor.IsWalking || actor.PendingStep is not null)
                    {
                        continue;
                    }

                    ApplyTilePosture(actor);
                }

                if (!changed.Contains(actor))
                {
                    changed.Add(actor);
                }
            }
        }

        return changes;
    }
}
