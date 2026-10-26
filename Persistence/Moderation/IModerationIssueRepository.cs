using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IModerationIssueRepository
{
    Task<IReadOnlyList<ModerationIssue>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<ModerationIssue> CreateAsync(int category, int reporterId, int? reportedId, int? roomId, string message, CancellationToken cancellationToken = default);

    Task<bool> TryPickAsync(int issueId, int moderatorId, CancellationToken cancellationToken = default);

    Task<bool> TryReleaseAsync(int issueId, int moderatorId, CancellationToken cancellationToken = default);

    Task<bool> TryCloseAsync(int issueId, int moderatorId, IssueResolution resolution, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> DeleteOpenByReporterAsync(int reporterId, CancellationToken cancellationToken = default);
}
