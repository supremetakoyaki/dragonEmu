using DragonEmu.Game.Moderation;

namespace DragonEmu.Protocol.Messages.Incoming.Help;

public sealed class DeletePendingCallsForHelpMessageComposer : IMessageComposer
{
    private readonly ModerationIssueService _issues;

    public DeletePendingCallsForHelpMessageComposer(ModerationIssueService issues)
    {
        _issues = issues;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _issues.DeletePendingAsync(session, account, cancellationToken);
        }
    }
}
