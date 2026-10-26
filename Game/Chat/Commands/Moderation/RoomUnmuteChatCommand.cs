using DragonEmu.Game.Moderation;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class RoomUnmuteChatCommand : IChatCommand
{
    private readonly ModerationService _moderation;
    private readonly IRoomManager _roomManager;
    private readonly MessageTexts _texts;

    public RoomUnmuteChatCommand(ModerationService moderation, IRoomManager roomManager, MessageTexts texts)
    {
        _texts = texts;
        _moderation = moderation;
        _roomManager = roomManager;
    }

    public string Name => "roomunmute";

    public string RequiredFuse => "fuse_room_mute";

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        if (session.CurrentRoomId is not { } roomId || !_roomManager.TryGetLoadedRoom(roomId, out Room? room))
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.NotInRoom)).Compose());
            return;
        }

        session.Send(new ModMessageEvent(await _moderation.UnmuteRoomAsync(room!, cancellationToken)).Compose());
    }
}
