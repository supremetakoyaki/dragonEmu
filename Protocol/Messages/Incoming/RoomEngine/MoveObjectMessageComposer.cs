using DragonEmu.Domain;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Incoming.RoomEngine.AuxiliaryFunctions;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine;

public sealed class MoveObjectMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly IUserItemRepository _userItemRepository;
    private readonly FurnitureDefinitionCache _furnitureDefinitions;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly RoomRightsService _roomRights;
    private readonly RoomMovementScheduler _scheduler;
    private readonly TotemService _totems;

    private int _itemId;
    private int _x;
    private int _y;
    private int _rawDirection;

    public MoveObjectMessageComposer(
        IRoomManager roomManager,
        IUserItemRepository userItemRepository,
        FurnitureDefinitionCache furnitureDefinitions,
        RoomBroadcaster roomBroadcaster,
        RoomRightsService roomRights,
        RoomMovementScheduler scheduler,
        TotemService totems)
    {
        _roomManager = roomManager;
        _userItemRepository = userItemRepository;
        _furnitureDefinitions = furnitureDefinitions;
        _roomBroadcaster = roomBroadcaster;
        _roomRights = roomRights;
        _scheduler = scheduler;
        _totems = totems;
    }

    public void Parse(ClientPacket packet)
    {
        _itemId = packet.PopInt32();
        _x = packet.PopInt32();
        _y = packet.PopInt32();
        _rawDirection = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || session.CurrentRoomId is not { } roomId)
        {
            return;
        }

        if (!_roomManager.TryGetLoadedRoom(roomId, out Room? room) || !await _roomRights.CanUseFurnitureAsync(account, room!.Data, cancellationToken))
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.Generic).Compose());
            return;
        }

        UserItem? item = await _userItemRepository.GetByIdAsync(_itemId, cancellationToken);
        if (item is null || item.Type != FurnitureType.Floor || item.RoomRef != roomId)
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.ItemNotFound).Compose());
            return;
        }

        FurnitureDefinition? definition = _furnitureDefinitions.GetById(item.FurniDefId);
        int rotation = (_rawDirection % 8) * 45;

        (int x, int y) = room.PlacementTile(_x, _y);

        if (room.GetFloorItem(_itemId) is not { } current)
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.ItemNotFound).Compose());
            return;
        }

        var exempt = new HashSet<long>(room.RestingActorsOn(current));

        var prospective = new RoomFloorItem(
            _itemId, x, y, 0, rotation, definition?.XDim ?? 1, definition?.YDim ?? 1,
            definition?.IsWalkable ?? false, definition?.IsSittable ?? false, definition?.IsBed ?? false,
            definition?.IsStackable ?? false, definition?.ZDim ?? 0);

        foreach ((int X, int Y) footTile in prospective.OccupiedTiles())
        {
            if (!room.Grid.CanHoldFurniture(footTile.X, footTile.Y))
            {
                session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.NotEnoughSpace).Compose());
                return;
            }
        }

        HashSet<(int X, int Y)> newFootprint = prospective.OccupiedTiles().ToHashSet();
        HashSet<(int X, int Y)> newLayTiles = prospective.IsBed ? prospective.LayTiles().ToHashSet() : [];

        foreach (long sessionId in room.StandingActorsOn(newFootprint))
        {
            if (exempt.Contains(sessionId) || !room.TryGetActor(sessionId, out RoomActor? standingActor))
            {
                continue;
            }

            bool allowed;
            lock (standingActor!.Lock)
            {
                allowed = prospective.IsSittable || (prospective.IsBed && newLayTiles.Contains((standingActor.X, standingActor.Y)));
            }

            if (!allowed)
            {
                session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.NotEnoughSpace).Compose());
                return;
            }

            exempt.Add(sessionId);
        }

        FloorTileResolver.Result tile = FloorTileResolver.Resolve(
            room, x, y, placedItemIsWalkable: definition?.IsWalkable ?? false, excludeItemId: _itemId, exempt);
        if (!tile.Success)
        {
            session.Send(new PlaceObjectErrorMessageEvent(tile.Error).Compose());
            return;
        }

        (int fromX, int fromY, double fromZ, int fromRotation) = (current.X, current.Y, current.Z, current.Rotation);
        if (room.TryMoveFloorItemOnStack(_itemId, x, y, rotation, exempt) is not { } z)
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.NotEnoughSpace).Compose());
            return;
        }

        bool moved = await _userItemRepository.MoveOnFloorAsync(_itemId, roomId, x, y, (decimal)z, rotation, cancellationToken);
        if (!moved)
        {
            room.MoveFloorItem(_itemId, fromX, fromY, fromZ, fromRotation);
            room.ConfirmFloorItem(_itemId);
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.ItemNotFound).Compose());
            return;
        }

        room.ConfirmFloorItem(_itemId);
        _scheduler.MarkActive(room);

        UserItem? movedItem = await _userItemRepository.GetByIdAsync(_itemId, cancellationToken);
        if (movedItem is not null)
        {
            _roomBroadcaster.Broadcast(room, new ObjectUpdateMessageEvent(movedItem).Compose());
        }

        if (room.GetFloorItem(_itemId) is { IsTotemPiece: true })
        {
            await _totems.SettleAsync(room, cancellationToken);
        }

        var reOriented = new List<RoomActor>();
        foreach (long sessionId in exempt)
        {
            if (room.TryGetActor(sessionId, out RoomActor? seated))
            {
                lock (seated!.Lock)
                {
                    if (room.ApplyTilePosture(seated))
                    {
                        reOriented.Add(seated);
                    }
                }
            }
        }

        if (reOriented.Count > 0)
        {
            _roomBroadcaster.Broadcast(room, new UserUpdateMessageEvent(reOriented).Compose());
        }
    }
}
