using DragonEmu.Game.Furniture;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Incoming.RoomEngine.AuxiliaryFunctions;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class PickAllChatCommand : IChatCommand
{
    private readonly IRoomManager _roomManager;
    private readonly IUserItemRepository _userItemRepository;
    private readonly RoomRightsService _roomRights;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly TotemService _totems;
    private readonly MessageTexts _texts;

    public PickAllChatCommand(
        IRoomManager roomManager, IUserItemRepository userItemRepository, RoomRightsService roomRights, RoomBroadcaster roomBroadcaster,
        TotemService totems, MessageTexts texts)
    {
        _texts = texts;
        _roomManager = roomManager;
        _userItemRepository = userItemRepository;
        _roomRights = roomRights;
        _roomBroadcaster = roomBroadcaster;
        _totems = totems;
    }

    public string Name => "pickall";

    public string? RequiredFuse => null;

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account
            || session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room) || room is null)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.NotInRoom)).Compose());
            return;
        }

        if (!RoomRightsService.IsOwner(account, room.Data) && !await _roomRights.HoldsAnyRoomControllerFuseAsync(account, cancellationToken))
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.PickallOwnerOnly)).Compose());
            return;
        }

        IReadOnlyList<int> picked = await _userItemRepository.PickAllAsync(roomId, account.Id, cancellationToken);
        foreach (int itemId in picked)
        {
            if (room.GetFloorItem(itemId) is { } floorItem)
            {
                FloorItemRemoval.RemoveFromRoom(room, itemId, _roomBroadcaster);
                if (floorItem.IsTotemPiece)
                {
                    await _totems.PickedUpAsync(room, floorItem, cancellationToken);
                }
            }
            else
            {
                room.RemoveWallItem(itemId);
                _roomBroadcaster.Broadcast(room, new ItemRemoveMessageEvent(itemId).Compose());
            }
        }

        if (picked.Count > 0)
        {
            session.Send(new FurniListUpdateEvent().Compose());
        }
    }
}
