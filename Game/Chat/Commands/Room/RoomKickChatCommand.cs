using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class RoomKickChatCommand : IChatCommand
{
    private readonly RoomKickService _kicks;
    private readonly MessageTexts _texts;

    public RoomKickChatCommand(RoomKickService kicks, MessageTexts texts)
    {
        _texts = texts;
        _kicks = kicks;
    }

    public string Name => "roomkick";

    public string RequiredFuse => "fuse_room_kick";

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (await _kicks.KickEveryoneAsync(session, cancellationToken) is null)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.NotInRoom)).Compose());
        }
    }
}
