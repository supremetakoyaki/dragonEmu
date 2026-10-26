using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IUserAchievementRepository
{
    Task<AchievementProgress> GetProgressAsync(int userId, int achievementId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(int AchievementId, AchievementProgress Progress)>> GetAllAsync(int userId, CancellationToken cancellationToken = default);

    Task SetProgressAsync(int userId, int achievementId, int counter, int currentLevel, CancellationToken cancellationToken = default);
}
