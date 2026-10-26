using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface ICatalogPageBodyRepository
{
    Task<CatalogPageBody?> GetByPageIdAsync(int pageId, CancellationToken cancellationToken = default);
}
