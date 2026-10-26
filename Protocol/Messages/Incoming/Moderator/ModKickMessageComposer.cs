using DragonEmu.Game.Moderation;

namespace DragonEmu.Protocol.Messages.Incoming.Moderator;

public sealed class ModKickMessageComposer : IMessageComposer
{
    private readonly ModerationToolService _tool;

    private int _userId;
    private string _message = string.Empty;
    private string _issueMessage = string.Empty;

    public ModKickMessageComposer(ModerationToolService tool)
    {
        _tool = tool;
    }

    public void Parse(ClientPacket packet)
    {
        _userId = packet.PopInt32();
        _message = packet.PopString();
        _issueMessage = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.IsModerator && session.AuthenticatedUser is { } account)
        {
            await _tool.KickAsync(session, account, _userId, _message, _issueMessage, cancellationToken);
        }
    }
}
