using System.Collections.Concurrent;
using DragonEmu.Domain;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Help;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Moderation;

public sealed class ModerationIssueService : IHostedService
{
    public static readonly IReadOnlySet<int> Topics = new HashSet<int> { 101, 102, 103, 104, 105, 106 };

    public const int MinMessageLength = 30;

    public const int MaxMessageLength = 500;

    public static readonly TimeSpan AbusiveCooldown = TimeSpan.FromMinutes(10);

    private readonly ConcurrentDictionary<int, ModerationIssue> _issues = new();
    private readonly ConcurrentDictionary<int, DateTime> _abusiveUntil = new();
    private readonly SemaphoreSlim _calls = new(1, 1);
    private readonly IModerationIssueRepository _repository;
    private readonly IUserRepository _users;
    private readonly ClientSessionManager _sessions;
    private readonly ILogger<ModerationIssueService> _logger;

    public ModerationIssueService(
        IModerationIssueRepository repository, IUserRepository users, ClientSessionManager sessions, ILogger<ModerationIssueService> logger)
    {
        _repository = repository;
        _users = users;
        _sessions = sessions;
        _logger = logger;
    }

    public IReadOnlyList<ModerationIssue> Active => _issues.Values.OrderBy(issue => issue.Id).ToList();

    public ModerationIssue? Get(int issueId) => _issues.TryGetValue(issueId, out ModerationIssue? issue) ? issue : null;

    public bool IsCallerOf(int userId, string message) =>
        _issues.Values.Any(issue => issue.ReporterId == userId && issue.Message == message);

    public async Task CallForHelpAsync(IGameSession session, UserAccount account, string text, int topic, int reportedUserId, CancellationToken cancellationToken)
    {
        string message = text.Trim();
        if (!Topics.Contains(topic) || message.Length < MinMessageLength)
        {
            return;
        }

        message = message[..Math.Min(message.Length, MaxMessageLength)];
        int? reportedId = null;
        if (reportedUserId != 0)
        {
            if (reportedUserId == account.Id || await _users.GetByIdAsync(reportedUserId, cancellationToken) is null)
            {
                return;
            }

            reportedId = reportedUserId;
        }

        if (_abusiveUntil.TryGetValue(account.Id, out DateTime until))
        {
            if (until > DateTime.UtcNow)
            {
                session.Send(new CallForHelpResultMessageEvent(CallForHelpResultMessageEvent.HasAbusiveCall).Compose());
                return;
            }

            _abusiveUntil.TryRemove(new KeyValuePair<int, DateTime>(account.Id, until));
        }

        ModerationIssue issue;
        await _calls.WaitAsync(cancellationToken);
        try
        {
            if (_issues.Values.Any(pending => pending.ReporterId == account.Id))
            {
                session.Send(new CallForHelpResultMessageEvent(CallForHelpResultMessageEvent.TooManyPending).Compose());
                return;
            }

            issue = await _repository.CreateAsync(topic, account.Id, reportedId, session.CurrentRoomId, message, cancellationToken);
            _issues[issue.Id] = issue;
        }
        finally
        {
            _calls.Release();
        }

        session.Send(new CallForHelpResultMessageEvent(CallForHelpResultMessageEvent.Sent).Compose());
        ToModerators(new IssueInfoMessageEvent(issue, DateTime.UtcNow).Compose());
        _logger.LogDebug("Call for help {IssueId} from {Reporter} (topic {Topic}).", issue.Id, account.Username, topic);
    }

    public async Task DeletePendingAsync(IGameSession session, UserAccount account, CancellationToken cancellationToken)
    {
        IReadOnlyList<int> deleted;
        await _calls.WaitAsync(cancellationToken);
        try
        {
            deleted = await _repository.DeleteOpenByReporterAsync(account.Id, cancellationToken);
            foreach (int issueId in deleted)
            {
                _issues.TryRemove(issueId, out _);
            }
        }
        finally
        {
            _calls.Release();
        }

        foreach (int issueId in deleted)
        {
            ToModerators(new IssueDeletedMessageEvent(issueId).Compose());
        }

        session.Send(_issues.Values.Any(pending => pending.ReporterId == account.Id)
            ? new CallForHelpResultMessageEvent(CallForHelpResultMessageEvent.TooManyPending).Compose()
            : new CallForHelpPendingCallsDeletedMessageEvent().Compose());
    }

    public async Task PickAsync(IGameSession session, UserAccount moderator, IReadOnlyList<int> issueIds, CancellationToken cancellationToken)
    {
        foreach (int issueId in issueIds.Distinct())
        {
            if (_issues.TryGetValue(issueId, out ModerationIssue? issue) && await _repository.TryPickAsync(issueId, moderator.Id, cancellationToken))
            {
                Update(issue with { State = IssueState.Picked, PickerId = moderator.Id, PickerName = moderator.Username });
            }
            else
            {
                ModerationIssue? current = Get(issueId);
                session.Send(new IssuePickFailedMessageEvent(issueId, current?.PickerId ?? 0, current?.PickerName ?? string.Empty).Compose());
            }
        }
    }

    public async Task ReleaseAsync(UserAccount moderator, IReadOnlyList<int> issueIds, CancellationToken cancellationToken)
    {
        foreach (int issueId in issueIds.Distinct())
        {
            if (_issues.TryGetValue(issueId, out ModerationIssue? issue) && await _repository.TryReleaseAsync(issueId, moderator.Id, cancellationToken))
            {
                Update(issue with { State = IssueState.Open, PickerId = null, PickerName = string.Empty });
            }
        }
    }

    public async Task CloseAsync(UserAccount moderator, IReadOnlyList<int> issueIds, IssueResolution resolution, CancellationToken cancellationToken)
    {
        foreach (int issueId in issueIds.Distinct())
        {
            if (!_issues.TryGetValue(issueId, out ModerationIssue? issue) || !await _repository.TryCloseAsync(issueId, moderator.Id, resolution, cancellationToken))
            {
                continue;
            }

            _issues.TryRemove(issueId, out _);
            ToModerators(new IssueDeletedMessageEvent(issueId).Compose());
            if (resolution == IssueResolution.Abusive)
            {
                DateTime now = DateTime.UtcNow;
                foreach (KeyValuePair<int, DateTime> expired in _abusiveUntil.Where(cooldown => cooldown.Value <= now))
                {
                    _abusiveUntil.TryRemove(expired);
                }

                _abusiveUntil[issue.ReporterId] = now + AbusiveCooldown;
            }

            if (_sessions.TryGetByUserId(issue.ReporterId, out ClientSession? reporter))
            {
                reporter!.EnqueueSend(new IssueCloseNotificationMessageEvent(resolution).Compose());
            }
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (ModerationIssue issue in await _repository.GetActiveAsync(cancellationToken))
        {
            _issues[issue.Id] = issue;
        }

        _logger.LogInformation("Moderation issues loaded: {Count} open or picked.", _issues.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void Update(ModerationIssue issue)
    {
        _issues[issue.Id] = issue;
        ToModerators(new IssueInfoMessageEvent(issue, DateTime.UtcNow).Compose());
    }

    private void ToModerators(ServerPacket packet)
    {
        byte[] frame = packet.ToArray();
        foreach (ClientSession moderator in _sessions.GetAuthenticatedSessions().Where(session => session.IsModerator))
        {
            moderator.EnqueueSend(frame);
        }
    }
}
