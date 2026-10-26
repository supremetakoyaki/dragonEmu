using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IGameSettingsRepository
{
    Task<GameSettings> GetAsync(CancellationToken cancellationToken = default);

    Task SetInfobusOpenAsync(bool open, CancellationToken cancellationToken = default);
}
