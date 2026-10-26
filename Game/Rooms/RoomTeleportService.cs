using DragonEmu.Domain;
using DragonEmu.Persistence;

namespace DragonEmu.Game.Rooms;

public sealed class RoomTeleportService
{
    private readonly ITeleportPairRepository _teleportPairRepository;
    private readonly IUserItemRepository _userItemRepository;
    private readonly RoomMovementScheduler _scheduler;

    public RoomTeleportService(
        ITeleportPairRepository teleportPairRepository, IUserItemRepository userItemRepository, RoomMovementScheduler scheduler)
    {
        _teleportPairRepository = teleportPairRepository;
        _userItemRepository = userItemRepository;
        _scheduler = scheduler;
    }

    public async Task<bool> TryUseAsync(Room room, RoomActor actor, RoomFloorItem teleporter, CancellationToken cancellationToken = default)
    {
        if (!room.CanStartTeleport(actor, teleporter.Id))
        {
            (int frontX, int frontY) = teleporter.SquareInFront();
            if (room.TryStartWalk(actor, frontX, frontY))
            {
                _scheduler.MarkActive(room);
            }

            return false;
        }

        TeleportDestination destination = await ResolveDestinationAsync(room.Id, teleporter.Id, cancellationToken);
        if (!room.TryStartTeleport(actor, teleporter.Id, destination))
        {
            return false;
        }

        _scheduler.MarkActive(room);
        return true;
    }

    public RoomFloorItem? FindArrivalTeleporter(Room room, PendingTeleportArrival arrival, long sessionId)
    {
        RoomFloorItem? teleporter = room.GetFloorItem(arrival.TeleporterItemId);
        return teleporter is not null && room.CanArriveAt(teleporter, sessionId) ? teleporter : null;
    }

    public void BeginArrival(Room room, RoomActor actor, RoomFloorItem teleporter)
    {
        if (room.TryBeginTeleportArrival(actor, teleporter.Id))
        {
            _scheduler.MarkActive(room);
        }
    }

    private async Task<TeleportDestination> ResolveDestinationAsync(int roomId, int teleporterItemId, CancellationToken cancellationToken)
    {
        IReadOnlyList<TeleportPair> pairs = await _teleportPairRepository.GetByItemIdAsync(teleporterItemId, cancellationToken);
        if (pairs.Count == 0)
        {
            return TeleportDestination.None;
        }

        TeleportPair pair = pairs[0];
        int pairItemId = pair.RefItem1 == teleporterItemId ? pair.RefItem2 : pair.RefItem1;

        UserItem? pairItem = await _userItemRepository.GetByIdAsync(pairItemId, cancellationToken);
        if (pairItem?.RoomRef is not { } pairRoomId)
        {
            return TeleportDestination.None;
        }

        return pairRoomId == roomId
            ? TeleportDestination.SameRoom(roomId, pairItemId)
            : TeleportDestination.OtherRoom(pairRoomId, pairItemId);
    }
}
