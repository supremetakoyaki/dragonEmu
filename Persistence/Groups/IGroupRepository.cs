using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IGroupRepository
{
    Task<GroupDetails?> GetDetailsAsync(int groupId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GroupBadge>> GetBadgesAsync(IReadOnlyCollection<int> groupIds, CancellationToken cancellationToken = default);
}
