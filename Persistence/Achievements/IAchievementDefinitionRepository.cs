using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IAchievementDefinitionRepository
{
    Task<IReadOnlyList<AchievementDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
}
