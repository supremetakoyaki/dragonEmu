using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Trading;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine;

public sealed class ApplyRoomEffectMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly IUserItemRepository _userItemRepository;
    private readonly RoomPaintService _roomPaintService;
    private readonly RoomRightsService _roomRights;
    private readonly TradingService _trading;

    private int _itemId;

    public ApplyRoomEffectMessageComposer(
        IRoomManager roomManager, IUserItemRepository userItemRepository, RoomPaintService roomPaintService, RoomRightsService roomRights,
        TradingService trading)
    {
        _roomManager = roomManager;
        _userItemRepository = userItemRepository;
        _roomPaintService = roomPaintService;
        _roomRights = roomRights;
        _trading = trading;
    }

    public void Parse(ClientPacket packet)
    {
        _itemId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || session.CurrentRoomId is not { } roomId)
        {
            return;
        }

        if (!_roomManager.TryGetLoadedRoom(roomId, out Room? room) || !await _roomRights.CanEditAsync(account, room!.Data, cancellationToken))
        {
            return;
        }

        UserItem? item = await _userItemRepository.GetByIdAsync(_itemId, cancellationToken);
        if (item is null || item.RoomRef is not null || string.IsNullOrEmpty(item.ExtraData)
            || RoomPaintService.PaintTypeFor(item.Classname) is not { } paintType || _trading.IsOffered(session, _itemId))
        {
            return;
        }

        if (!await _userItemRepository.DeleteFromInventoryAsync(_itemId, account.Id, cancellationToken))
        {
            return;
        }

        await _roomPaintService.ApplyAsync(room, paintType, item.ExtraData, cancellationToken);
        session.Send(new FurniListRemoveEvent(_itemId).Compose());
    }
}
