using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IDimmerPresetRepository
{
    Task<IReadOnlyList<DimmerPreset>> GetPresetsAsync(int itemId, CancellationToken cancellationToken = default);

    Task SavePresetAsync(int itemId, DimmerPreset preset, CancellationToken cancellationToken = default);
}
