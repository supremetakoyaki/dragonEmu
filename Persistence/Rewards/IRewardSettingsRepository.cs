using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IRewardSettingsRepository
{
    Task<OnlineRewardSettings> GetAsync(CancellationToken cancellationToken = default);
}
