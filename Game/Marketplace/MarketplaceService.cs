using DragonEmu.Domain;
using DragonEmu.Game.Fuses;
using DragonEmu.Game.Settings;
using DragonEmu.Game.Trading;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.InventoryPurse;
using DragonEmu.Protocol.Messages.Outgoing.Marketplace;

namespace DragonEmu.Game.Marketplace;

public sealed class MarketplaceService
{
    public const int MaxListed = 250;

    public const int MaxOwnListed = 250;

    public const int HistoryDays = 30;

    public const int MaxSearchLength = 40;

    public static readonly TimeSpan SearchInterval = TimeSpan.FromMilliseconds(500);

    private const string SearchCooldown = "marketplace_search";

    private const string SearchPlaceholder = "Search by name...";

    private readonly IMarketplaceRepository _repository;
    private readonly IUserItemRepository _items;
    private readonly GameSettingsCache _gameSettings;
    private readonly FuseService _fuses;
    private readonly TradingService _trading;
    private readonly HotelClock _clock;

    public MarketplaceService(
        IMarketplaceRepository repository, IUserItemRepository items, GameSettingsCache gameSettings, FuseService fuses, TradingService trading,
        HotelClock clock, HotelStatus hotel)
    {
        _hotel = hotel;
        _repository = repository;
        _items = items;
        _gameSettings = gameSettings;
        _fuses = fuses;
        _trading = trading;
        _clock = clock;
    }

    private readonly HotelStatus _hotel;

    private bool Enabled => _gameSettings.Current.MarketplaceEnabled && !_hotel.IsShuttingDown;

    public async Task SendConfigAsync(IGameSession session, CancellationToken cancellationToken) =>
        session.Send(new MarketplaceConfigurationEvent(Enabled, await _repository.GetConfigAsync(cancellationToken)).Compose());

    public async Task SendCanMakeOfferAsync(IGameSession session, UserAccount account, CancellationToken cancellationToken)
    {
        int tokens = await _repository.GetTokensAsync(account.Id, cancellationToken);
        MarketplaceCanMakeOfferResult result = await MaySellAsync(account, cancellationToken);
        if (result == MarketplaceCanMakeOfferResult.Yes && tokens <= 0)
        {
            result = MarketplaceCanMakeOfferResult.NoTokens;
        }

        session.Send(new MarketplaceCanMakeOfferResultEvent(result, tokens).Compose());
    }

    public async Task BuyTokensAsync(IGameSession session, UserAccount account, CancellationToken cancellationToken)
    {
        if (await MaySellAsync(account, cancellationToken) is var allowed and not MarketplaceCanMakeOfferResult.Yes)
        {
            session.Send(new MarketplaceCanMakeOfferResultEvent(allowed, 0).Compose());
            return;
        }

        MarketplaceConfig config = await _repository.GetConfigAsync(cancellationToken);
        if (await _repository.BuyTokensAsync(account.Id, config.TokenBatchPrice, config.TokenBatchSize, cancellationToken) is not { } bought)
        {
            session.Send(new NotEnoughBalanceMessageEvent(notEnoughCredits: true, notEnoughPixels: false).Compose());
            return;
        }

        session.Send(new CreditBalanceEvent(bought.Credits).Compose());
        session.Send(new MarketplaceCanMakeOfferResultEvent(MarketplaceCanMakeOfferResult.Yes, bought.Tokens).Compose());
    }

    public async Task MakeOfferAsync(IGameSession session, UserAccount account, int askingPrice, int itemId, CancellationToken cancellationToken)
    {
        MarketplaceMakeOfferResult result = await PostAsync(session, account, askingPrice, itemId, cancellationToken);
        session.Send(new MarketplaceMakeOfferResultEvent(result).Compose());
        if (result == MarketplaceMakeOfferResult.Posted)
        {
            session.Send(new FurniListRemoveEvent(itemId).Compose());
        }
    }

    private async Task<MarketplaceMakeOfferResult> PostAsync(IGameSession session, UserAccount account, int askingPrice, int itemId, CancellationToken cancellationToken)
    {
        if (!Enabled)
        {
            return MarketplaceMakeOfferResult.Disabled;
        }

        MarketplaceConfig config = await _repository.GetConfigAsync(cancellationToken);
        if (await MaySellAsync(account, cancellationToken) != MarketplaceCanMakeOfferResult.Yes || !config.AllowsAskingPrice(askingPrice)
            || _trading.IsOffered(session, itemId))
        {
            return MarketplaceMakeOfferResult.TechnicalError;
        }

        DateTime expiresAt = DateTime.UtcNow.ToWholeSecond().AddHours(config.OfferExpirationHours);
        return await _repository.CreateOfferAsync(
            account.Id, itemId, askingPrice, config.BuyerPrice(askingPrice), expiresAt, refuseCatalogueFurniture: !config.CanSellWhatsInCatalog,
            cancellationToken) switch
        {
            MarketplaceOfferCreation.Created => MarketplaceMakeOfferResult.Posted,
            MarketplaceOfferCreation.InCatalogue => MarketplaceMakeOfferResult.InCatalogue,
            _ => MarketplaceMakeOfferResult.TechnicalError,
        };
    }

    public async Task SearchAsync(IGameSession session, UserAccount account, int minPrice, int maxPrice, string name, int sort, CancellationToken cancellationToken)
    {
        if (!session.TryUseCooldown(SearchCooldown, SearchInterval))
        {
            await Task.Delay(SearchInterval, cancellationToken);
            session.TryUseCooldown(SearchCooldown, SearchInterval);
        }

        if (!Enabled)
        {
            session.Send(new MarketPlaceOffersEvent([]).Compose());
            return;
        }

        MarketplaceConfig config = await _repository.GetConfigAsync(cancellationToken);
        IReadOnlyList<MarketplaceListing> listings = await _repository.SearchAsync(
            account.Id, Math.Max(-1, minPrice), Math.Max(-1, maxPrice), CleanName(name),
            Enum.IsDefined((MarketplaceSort)sort) ? (MarketplaceSort)sort : MarketplaceSort.MostExpensive,
            config.AveragePricePeriodDays, _clock.DayStartsUtc().Today, MaxListed, cancellationToken);
        session.Send(new MarketPlaceOffersEvent(listings).Compose());
    }

    public async Task SendOwnOffersAsync(IGameSession session, UserAccount account, CancellationToken cancellationToken)
    {
        MarketplaceConfig config = await _repository.GetConfigAsync(cancellationToken);
        session.Send(new MarketPlaceOwnOffersEvent(
            await _repository.GetOwnOffersAsync(account.Id, config.AveragePricePeriodDays, MaxOwnListed, cancellationToken)).Compose());
    }

    public async Task SendItemStatsAsync(IGameSession session, UserAccount account, int furniType, int sprite, CancellationToken cancellationToken)
    {
        if (furniType is not (MarketplaceFurniType.Floor or MarketplaceFurniType.Wall))
        {
            return;
        }

        MarketplaceConfig config = await _repository.GetConfigAsync(cancellationToken);
        (int average, int offers, IReadOnlyList<MarketplaceSale> sales) = await _repository.GetItemStatsAsync(
            account.Id, furniType, sprite, config.AveragePricePeriodDays, DateTime.UtcNow.AddDays(-(HistoryDays + 1)), cancellationToken);
        session.Send(new MarketplaceItemStatsEvent(
            new MarketplaceItemStats(average, offers, HistoryDays, Days(sales, _clock.Today, _clock.ToHotelTime), furniType, sprite)).Compose());
    }

    public static IReadOnlyList<MarketplaceDay> Days(IEnumerable<MarketplaceSale> sales, DateOnly today, Func<DateTime, DateTime> toHotelTime) =>
        sales
            .GroupBy(sale => DateOnly.FromDateTime(toHotelTime(sale.SoldAt)).DayNumber - today.DayNumber)
            .Where(day => day.Key is >= -HistoryDays and <= 0)
            .OrderBy(day => day.Key)
            .Select(day => new MarketplaceDay(day.Key, (int)Math.Round(day.Average(sale => sale.Price)), day.Count()))
            .ToList();

    public async Task BuyAsync(IGameSession session, UserAccount account, int offerId, CancellationToken cancellationToken)
    {
        MarketplacePurchase purchase = Enabled
            ? await _repository.BuyAsync(account.Id, offerId, cancellationToken)
            : new MarketplacePurchase(MarketplaceBuyResult.SoldOut, 0, 0);

        session.Send(new MarketplaceBuyOfferResultEvent(purchase, offerId).Compose());
        if (purchase.Result != MarketplaceBuyResult.Bought)
        {
            return;
        }

        session.Send(new CreditBalanceEvent(purchase.CreditsLeft).Compose());
        if (purchase.ItemId is { } itemId && await _items.GetByIdAsync(itemId, cancellationToken) is { } item)
        {
            session.Send(new FurniListInsertEvent(item).Compose());
        }
    }

    public async Task CancelAsync(IGameSession session, UserAccount account, int offerId, CancellationToken cancellationToken)
    {
        int? itemId = await _repository.CancelAsync(account.Id, offerId, cancellationToken);
        session.Send(new MarketplaceCancelOfferResultEvent(offerId, itemId is not null).Compose());
        if (itemId is { } id && await _items.GetByIdAsync(id, cancellationToken) is { } item)
        {
            session.Send(new FurniListInsertEvent(item).Compose());
        }
    }

    public async Task RedeemAsync(IGameSession session, UserAccount account, CancellationToken cancellationToken)
    {
        (int credits, long balance) = await _repository.RedeemAsync(account.Id, cancellationToken);
        if (credits > 0)
        {
            session.Send(new CreditBalanceEvent(balance).Compose());
        }
    }

    private async Task<MarketplaceCanMakeOfferResult> MaySellAsync(UserAccount account, CancellationToken cancellationToken)
    {
        if (!Enabled || !_gameSettings.Current.TradingEnabled)
        {
            return MarketplaceCanMakeOfferResult.NotAllowed;
        }

        return await _fuses.HoldsAsync(account, TradingService.TradeFuse, cancellationToken: cancellationToken)
            ? MarketplaceCanMakeOfferResult.Yes
            : MarketplaceCanMakeOfferResult.NoTradingPass;
    }

    private static string CleanName(string name)
    {
        string text = new string(name.Where(c => !char.IsControl(c)).ToArray()).Trim();
        if (string.Equals(text, SearchPlaceholder, StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return text.Length <= MaxSearchLength ? text : text[..MaxSearchLength];
    }
}
