using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine.AuxiliaryFunctions;

internal static class FloorTileResolver
{
    public readonly record struct Result(bool Success, decimal Z, PlaceObjectErrorReason Error)
    {
        public static Result Ok(decimal z) => new(true, z, PlaceObjectErrorReason.Generic);
        public static Result Fail(PlaceObjectErrorReason error) => new(false, 0m, error);
    }

    public static Result Resolve(
        Room room, int x, int y, bool placedItemIsWalkable, int excludeItemId,
        IReadOnlySet<long>? exemptOccupants = null)
    {
        if (!room.Grid.CanHoldFurniture(x, y))
        {
            return Result.Fail(PlaceObjectErrorReason.NotEnoughSpace);
        }

        HashSet<(int X, int Y)> occupied = exemptOccupants is { Count: > 0 }
            ? room.OccupiedTiles(exemptOccupants)
            : room.OccupiedTiles(excludeSessionId: -1);

        if (!placedItemIsWalkable && occupied.Contains((x, y)))
        {
            return Result.Fail(PlaceObjectErrorReason.NotEnoughSpace);
        }

        return room.StackTopAt(x, y, excludeItemId) is { } z
            ? Result.Ok((decimal)z)
            : Result.Fail(PlaceObjectErrorReason.NotEnoughSpace);
    }
}
