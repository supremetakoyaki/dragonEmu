using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface ITraxDefinitionRepository
{
    Task<IReadOnlyList<TraxSoundSet>> GetSoundSetsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TraxSample>> GetSamplesAsync(CancellationToken cancellationToken = default);
}
