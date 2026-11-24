using DragonEmu.Game.Moderation;

namespace DragonEmu.Protocol.Messages.Incoming.Moderator;

public sealed class PickIssuesMessageComposer : IMessageComposer
{
    private const int MaxIssues = 100;

    private readonly ModerationIssueService _issues;

    private readonly List<int> _issueIds = [];
    private bool _retryEnabled;
    private int _retryCount;

    public PickIssuesMessageComposer(ModerationIssueService issues)
    {
        _issues = issues;
    }

    public void Parse(ClientPacket packet)
    {
        int count = Math.Clamp(packet.PopInt32(), 0, MaxIssues);
        for (int i = 0; i < count; i++)
        {
            _issueIds.Add(packet.PopInt32());
        }

        _retryEnabled = packet.PopBoolean();
        _retryCount = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.IsModerator && session.AuthenticatedUser is { } account)
        {
            await _issues.PickAsync(session, account, _issueIds, _retryEnabled, _retryCount, cancellationToken);
        }
    }
}
