using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class RoomAlertChatCommand : IChatCommand
{
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;
    private readonly MessageTexts _texts;

    public RoomAlertChatCommand(IRoomManager roomManager, RoomBroadcaster broadcaster, MessageTexts texts)
    {
        _texts = texts;
        _roomManager = roomManager;
        _broadcaster = broadcaster;
    }

    public string Name => "roomalert";

    public string RequiredFuse => "fuse_room_alert";

    public ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.UsageRoomalert)).Compose());
        }
        else if (session.CurrentRoomId is not { } roomId || !_roomManager.TryGetLoadedRoom(roomId, out Room? room))
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.NotInRoom)).Compose());
        }
        else
        {
            _broadcaster.Broadcast(room!, new ModMessageEvent(string.Join(' ', args)).Compose());
        }

        return ValueTask.CompletedTask;
    }
}
