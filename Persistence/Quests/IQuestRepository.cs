using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IQuestRepository
{
    Task<IReadOnlyList<QuestDefinition>> GetCampaignAsync(string campaign, int userId, CancellationToken cancellationToken = default);

    Task AcceptAsync(int userId, int questId, CancellationToken cancellationToken = default);

    Task<bool> CompleteAsync(int userId, int questId, CancellationToken cancellationToken = default);
}
