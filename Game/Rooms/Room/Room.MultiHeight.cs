namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    public IReadOnlyList<RoomActor> SetHeightState(int itemId, int state)
    {
        HashSet<(int X, int Y)> tiles;
        lock (_furnitureLock)
        {
            if (!_floorItems.TryGetValue(itemId, out RoomFloorItem? item) || item.Heights is not { Count: > 0 })
            {
                return [];
            }

            item.HeightIndex = RoomFloorItem.HeightIndexOf(item.States, state.ToString(System.Globalization.CultureInfo.InvariantCulture));
            tiles = item.OccupiedTiles().ToHashSet();
            RebuildTileIndex();
        }

        var settled = new List<RoomActor>();
        foreach (long sessionId in ActorsOn(tiles))
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
                settled.Add(actor);
            }
        }

        return settled;
    }
}
