using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface ICatalogOfferRepository
{
    Task<IReadOnlyList<CatalogOffer>> GetByPageIdAsync(int pageId, CancellationToken cancellationToken = default);
}
