using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IFigureDataRepository
{
    Task<IReadOnlyList<FigureSettype>> GetSettypesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FigureSet>> GetSetsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FigureColor>> GetColorsAsync(CancellationToken cancellationToken = default);
}
