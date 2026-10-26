using DragonEmu.Game.Moderation;
using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Incoming.Moderator;

public sealed class CloseIssuesMessageComposer : IMessageComposer
{
    private const int MaxIssues = 100;

    private readonly ModerationIssueService _issues;

    private readonly List<int> _issueIds = [];
    private int _resolution;

    public CloseIssuesMessageComposer(ModerationIssueService issues)
    {
        _issues = issues;
    }

    public void Parse(ClientPacket packet)
    {
        _resolution = packet.PopInt32();
        int count = Math.Clamp(packet.PopInt32(), 0, MaxIssues);
        for (int i = 0; i < count; i++)
        {
            _issueIds.Add(packet.PopInt32());
        }
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.IsModerator && session.AuthenticatedUser is { } account && _resolution is >= 1 and <= 3)
        {
            await _issues.CloseAsync(account, _issueIds, (IssueResolution)_resolution, cancellationToken);
        }
    }
}
