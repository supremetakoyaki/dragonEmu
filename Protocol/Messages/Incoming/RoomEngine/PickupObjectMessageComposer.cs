using DragonEmu.Domain;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Incoming.RoomEngine.AuxiliaryFunctions;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine;

public sealed class PickupObjectMessageComposer : IMessageComposer
{
    private const int WallCategoryCode = 1;
    private const int FloorCategoryCode = 2;

    private readonly IRoomManager _roomManager;
    private readonly IUserItemRepository _userItemRepository;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly RoomRightsService _roomRights;
    private readonly TotemService _totems;

    private int _categoryCode;
    private int _itemId;

    public PickupObjectMessageComposer(
        IRoomManager roomManager, IUserItemRepository userItemRepository, RoomBroadcaster roomBroadcaster, RoomRightsService roomRights,
        TotemService totems)
    {
        _roomManager = roomManager;
        _userItemRepository = userItemRepository;
        _roomBroadcaster = roomBroadcaster;
        _roomRights = roomRights;
        _totems = totems;
    }

    public void Parse(ClientPacket packet)
    {
        _categoryCode = packet.PopInt32();
        _itemId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || session.CurrentRoomId is not { } roomId)
        {
            return;
        }

        if (!_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || !await _roomRights.CanPickUpAsync(account, room!.Data, isWallItem: _categoryCode == WallCategoryCode, cancellationToken))
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.Generic).Compose());
            return;
        }

        if (_categoryCode == WallCategoryCode)
        {
            await HandleWallPickupAsync(session, account, room!, roomId, cancellationToken);
            return;
        }

        if (_categoryCode != FloorCategoryCode)
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

        bool pickedUp = await _userItemRepository.PickupFromFloorAsync(_itemId, roomId, account.Id, cancellationToken);
        if (!pickedUp)
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.ItemNotFound).Compose());
            return;
        }

        RoomFloorItem? removed = room.GetFloorItem(_itemId);
        FloorItemRemoval.RemoveFromRoom(room, _itemId, _roomBroadcaster);

        if (removed is { IsTotemPiece: true })
        {
            await _totems.PickedUpAsync(room, removed, cancellationToken);
        }

        await SendToInventoryAsync(session, cancellationToken);
    }

    private async ValueTask HandleWallPickupAsync(IGameSession session, UserAccount account, Room room, int roomId, CancellationToken cancellationToken)
    {
        UserItem? item = await _userItemRepository.GetByIdAsync(_itemId, cancellationToken);
        if (item is null || item.Type != FurnitureType.Wall || item.RoomRef != roomId)
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.ItemNotFound).Compose());
            return;
        }

        if (item.LogicClass == FurnitureLogicClass.Stickie)
        {
            return;
        }

        bool pickedUp = await _userItemRepository.PickupFromWallAsync(_itemId, roomId, account.Id, cancellationToken);
        if (!pickedUp)
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.ItemNotFound).Compose());
            return;
        }

        room.RemoveWallItem(_itemId);
        _roomBroadcaster.Broadcast(room, new ItemRemoveMessageEvent(_itemId).Compose());

        await SendToInventoryAsync(session, cancellationToken);
    }

    private async ValueTask SendToInventoryAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (await _userItemRepository.GetByIdAsync(_itemId, cancellationToken) is { } pickedUp)
        {
            session.Send(new FurniListInsertEvent(pickedUp).Compose());
        }
    }
}
