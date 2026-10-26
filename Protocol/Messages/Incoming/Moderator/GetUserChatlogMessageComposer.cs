using DragonEmu.Game.Moderation;

namespace DragonEmu.Protocol.Messages.Incoming.Moderator;

public sealed class GetUserChatlogMessageComposer : IMessageComposer
{
    private readonly ModerationToolService _tool;

    private int _userId;

    public GetUserChatlogMessageComposer(ModerationToolService tool)
    {
        _tool = tool;
    }

    public void Parse(ClientPacket packet)
    {
        _userId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.IsModerator && session.AuthenticatedUser is { } account)
        {
            await _tool.SendUserChatlogAsync(session, account, _userId, cancellationToken);
        }
    }
}
