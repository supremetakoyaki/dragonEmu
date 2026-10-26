using DragonEmu.Game.Moderation;

namespace DragonEmu.Protocol.Messages.Incoming.Moderator;

public sealed class GetRoomChatlogMessageComposer : IMessageComposer
{
    private readonly ModerationToolService _tool;

    private int _roomId;

    public GetRoomChatlogMessageComposer(ModerationToolService tool)
    {
        _tool = tool;
    }

    public void Parse(ClientPacket packet)
    {
        packet.PopInt32();
        _roomId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.IsModerator && session.AuthenticatedUser is { } account)
        {
            await _tool.SendRoomChatlogAsync(session, account, _roomId, cancellationToken);
        }
    }
}
