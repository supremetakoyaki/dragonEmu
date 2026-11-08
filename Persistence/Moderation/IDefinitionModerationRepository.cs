using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IDefinitionModerationRepository
{
    Task<IReadOnlyDictionary<Language, ModerationPresets>> GetPresetsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Language, IReadOnlyList<FaqCategory>>> GetFaqAsync(CancellationToken cancellationToken = default);
}
