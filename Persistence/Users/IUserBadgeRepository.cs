using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IUserBadgeRepository
{
    Task<bool> HasBadgeAsync(int userId, string badgeCode, CancellationToken cancellationToken = default);

    Task<bool> AwardAsync(int userId, string badgeCode, CancellationToken cancellationToken = default);

    Task RemoveAsync(int userId, string badgeCode, CancellationToken cancellationToken = default);

    Task<int?> ReplaceAsync(int userId, string oldBadgeCode, string newBadgeCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserBadge>> GetAllAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(int Slot, string BadgeCode)>> GetWornAsync(int userId, CancellationToken cancellationToken = default);

    Task SetWornAsync(int userId, IReadOnlyList<(int Slot, string BadgeCode)> worn, CancellationToken cancellationToken = default);
}
