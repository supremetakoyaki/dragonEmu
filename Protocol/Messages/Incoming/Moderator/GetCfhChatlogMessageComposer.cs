using DragonEmu.Game.Moderation;

namespace DragonEmu.Protocol.Messages.Incoming.Moderator;

public sealed class GetCfhChatlogMessageComposer : IMessageComposer
{
    private readonly ModerationToolService _tool;

    private int _issueId;

    public GetCfhChatlogMessageComposer(ModerationToolService tool)
    {
        _tool = tool;
    }

    public void Parse(ClientPacket packet)
    {
        _issueId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.IsModerator && session.AuthenticatedUser is { } account)
        {
            await _tool.SendCallChatlogAsync(session, account, _issueId, cancellationToken);
        }
    }
}
