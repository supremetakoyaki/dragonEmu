namespace DragonEmu.Domain;

public sealed record MarketplaceConfig(
    int CommissionPercent,
    int TokenBatchPrice,
    int TokenBatchSize,
    int OfferMinPrice,
    int OfferMaxPrice,
    int OfferExpirationHours,
    int AveragePricePeriodDays,
    bool CanSellWhatsInCatalog)
{
    public static readonly MarketplaceConfig Default = new(1, 1, 5, 1, 10000, 48, 7, true);

    public int CommissionOn(int askingPrice) => Math.Max((int)Math.Ceiling(CommissionPercent * 0.01 * askingPrice), 1);

    public int BuyerPrice(int askingPrice) => askingPrice + CommissionOn(askingPrice);

    public bool AllowsAskingPrice(int askingPrice) => askingPrice >= OfferMinPrice && askingPrice <= OfferMaxPrice;
}

public static class MarketplaceFurniType
{
    public const int Floor = 1;
    public const int Wall = 2;

    public static int Of(FurnitureType type) => type == FurnitureType.Floor ? Floor : Wall;
}

public enum MarketplaceOfferStatus
{
    Open = 1,
    Sold = 2,
    Expired = 3,
}

public enum MarketplaceSort
{
    MostExpensive = 1,
    Cheapest = 2,
    MostTradedToday = 3,
    LeastTradedToday = 4,
    MostOffers = 5,
    LeastOffers = 6,
}

public sealed record MarketplaceListing(int OfferId, int FurniType, int Sprite, int Price, int MinutesLeft, int AveragePrice, int OfferCount);

public sealed record MarketplaceOwnOffer(int OfferId, MarketplaceOfferStatus Status, int FurniType, int Sprite, int Price, int MinutesLeft, int AveragePrice);

public sealed record MarketplaceOwnOffers(int CreditsWaiting, IReadOnlyList<MarketplaceOwnOffer> Offers);

public sealed record MarketplaceDay(int DayOffset, int AveragePrice, int Sold);

public sealed record MarketplaceItemStats(int AveragePrice, int OfferCount, int HistoryDays, IReadOnlyList<MarketplaceDay> Days, int FurniType, int Sprite);

public sealed record MarketplaceSale(DateTime SoldAt, int Price);

public enum MarketplaceMakeOfferResult
{
    Posted = 1,
    TechnicalError = 2,
    Disabled = 3,
    InCatalogue = 4,
}

public enum MarketplaceCanMakeOfferResult
{
    Yes = 1,
    NotAllowed = 2,
    NoTradingPass = 3,
    NoTokens = 4,
}

public enum MarketplaceBuyResult
{
    Bought = 1,
    SoldOut = 2,
    NewPrice = 3,
    NotEnoughCredits = 4,
}

public sealed record MarketplacePurchase(MarketplaceBuyResult Result, int OfferId, int Price, int? ItemId = null, long CreditsLeft = 0);
