using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Protocol.Messages.Outgoing.RoomAction;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class HandItemChatCommand : IChatCommand
{
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly RoomMovementScheduler _scheduler;
    private readonly MessageTexts _texts;

    public HandItemChatCommand(IRoomManager roomManager, RoomBroadcaster roomBroadcaster, RoomMovementScheduler scheduler, MessageTexts texts)
    {
        _texts = texts;
        _roomManager = roomManager;
        _roomBroadcaster = roomBroadcaster;
        _scheduler = scheduler;
    }

    public string Name => "handitem";

    public string RequiredFuse => RoomRightsService.AdministratorAccessFuse;

    public ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length != 1 || !int.TryParse(args[0], out int itemId) || itemId < 0)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.UsageHanditem)).Compose());
            return ValueTask.CompletedTask;
        }

        if (session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room) || room is null
            || !room.TryGetActor(session.Id, out RoomActor? actor) || actor is null)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.NotInRoom)).Compose());
            return ValueTask.CompletedTask;
        }

        Room.GiveHandItem(actor, itemId);
        _roomBroadcaster.Broadcast(room, new CarryObjectMessageEvent(actor.VirtualId, itemId).Compose());
        _scheduler.MarkActive(room);
        return ValueTask.CompletedTask;
    }
}
