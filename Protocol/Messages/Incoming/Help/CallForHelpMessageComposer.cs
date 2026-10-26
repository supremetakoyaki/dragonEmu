using DragonEmu.Game.Moderation;

namespace DragonEmu.Protocol.Messages.Incoming.Help;

public sealed class CallForHelpMessageComposer : IMessageComposer
{
    private readonly ModerationIssueService _issues;

    private string _message = string.Empty;
    private int _topic;
    private int _reportedUserId;

    public CallForHelpMessageComposer(ModerationIssueService issues)
    {
        _issues = issues;
    }

    public void Parse(ClientPacket packet)
    {
        _message = packet.PopString();
        packet.PopInt32();
        _topic = packet.PopInt32();
        _reportedUserId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _issues.CallForHelpAsync(session, account, _message, _topic, _reportedUserId, cancellationToken);
        }
    }
}
