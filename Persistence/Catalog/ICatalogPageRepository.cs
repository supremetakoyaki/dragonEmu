using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface ICatalogPageRepository
{
    Task<IReadOnlyList<CatalogPageSummary>> GetAllAsync(CancellationToken cancellationToken = default);
}
