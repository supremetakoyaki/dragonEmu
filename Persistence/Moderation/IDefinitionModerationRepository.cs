using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IDefinitionModerationRepository
{
    Task<ModerationPresets> GetPresetsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FaqCategory>> GetFaqAsync(CancellationToken cancellationToken = default);
}
