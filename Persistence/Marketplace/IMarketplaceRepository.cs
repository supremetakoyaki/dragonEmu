using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public enum MarketplaceOfferCreation
{
    Created,
    NotSellable,
    NoTokens,
    InCatalogue,
}

public interface IMarketplaceRepository
{
    Task<MarketplaceConfig> GetConfigAsync(CancellationToken cancellationToken = default);

    Task<int> GetTokensAsync(int userId, CancellationToken cancellationToken = default);

    Task<(long Credits, int Tokens)?> BuyTokensAsync(int userId, int price, int batchSize, CancellationToken cancellationToken = default);

    Task<MarketplaceOfferCreation> CreateOfferAsync(
        int userId, int itemId, int askingPrice, int price, DateTime expiresAt, bool refuseCatalogueFurniture, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MarketplaceListing>> SearchAsync(
        int viewerId, int minPrice, int maxPrice, string name, MarketplaceSort sort, int averageDays, DateTime todayStartUtc, int limit,
        CancellationToken cancellationToken = default);

    Task<MarketplaceOwnOffers> GetOwnOffersAsync(int sellerId, int averageDays, int limit, CancellationToken cancellationToken = default);

    Task<(int AveragePrice, int OfferCount, IReadOnlyList<MarketplaceSale> Sales)> GetItemStatsAsync(
        int viewerId, int furniType, int sprite, int averageDays, DateTime since, CancellationToken cancellationToken = default);

    Task<MarketplacePurchase> BuyAsync(int buyerId, int requestedOfferId, CancellationToken cancellationToken = default);

    Task<int?> CancelAsync(int sellerId, int offerId, CancellationToken cancellationToken = default);

    Task<(int Credits, long Balance)> RedeemAsync(int sellerId, CancellationToken cancellationToken = default);
}
