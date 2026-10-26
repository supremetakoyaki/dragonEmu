using DragonEmu.Game.Moderation;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class RoomMuteChatCommand : IChatCommand
{
    private readonly ModerationService _moderation;
    private readonly IRoomManager _roomManager;
    private readonly MessageTexts _texts;

    public RoomMuteChatCommand(ModerationService moderation, IRoomManager roomManager, MessageTexts texts)
    {
        _texts = texts;
        _moderation = moderation;
        _roomManager = roomManager;
    }

    public string Name => "roommute";

    public string RequiredFuse => "fuse_room_mute";

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        if (args.Length > 1 || !ModerationCommandArgs.TryDurationAndText(args, 0, out int? hours, out string? extra) || extra is not null)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.UsageRoommute)).Compose());
            return;
        }

        if (session.CurrentRoomId is not { } roomId || !_roomManager.TryGetLoadedRoom(roomId, out Room? room))
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.NotInRoom)).Compose());
            return;
        }

        session.Send(new ModMessageEvent(await _moderation.MuteRoomAsync(account, room!, hours, cancellationToken)).Compose());
    }
}
