using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;
using DragonEmu.Domain;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.History;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Catalog;
using DragonEmu.Game.Subscriptions;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;
using DragonEmu.Protocol.Messages.Outgoing.Handshake;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.InventoryPurse;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;
using DragonEmu.Protocol.Messages.Outgoing.Users;

namespace DragonEmu.Protocol.Messages.Incoming.Catalog;

public sealed class PurchaseFromCatalogAsGiftComposer : IMessageComposer
{
    private static readonly string[] FreeWrapClassnames =
        ["present_gen", "present_gen1", "present_gen2", "present_gen3", "present_gen4", "present_gen5", "present_gen6"];

    private readonly CatalogCache _catalogCache;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserItemRepository _userItemRepository;
    private readonly IUserGiftRepository _userGiftRepository;
    private readonly FurnitureDefinitionCache _furnitureDefinitions;
    private readonly SubscriptionManager _subscriptionManager;
    private readonly ClientSessionManager _sessionManager;
    private readonly AchievementService _achievementService;
    private readonly HistoryLog _history;

    private int _pageId;
    private int _offerId;
    private string _extraData = string.Empty;
    private string _receiverName = string.Empty;
    private string _giftMessage = string.Empty;
    private int _boxId;
    private int _colourValue;
    private int _ribbonId;

    private readonly HotelStatus _hotel;
    private readonly MessageTexts _texts;

    public PurchaseFromCatalogAsGiftComposer(
        CatalogCache catalogCache,
        ICurrencyRepository currencyRepository,
        IUserRepository userRepository,
        IUserItemRepository userItemRepository,
        IUserGiftRepository userGiftRepository,
        FurnitureDefinitionCache furnitureDefinitions,
        SubscriptionManager subscriptionManager,
        ClientSessionManager sessionManager,
        AchievementService achievementService,
        HistoryLog history,
        HotelStatus hotel, MessageTexts texts)
    {
        _texts = texts;
        _hotel = hotel;
        _catalogCache = catalogCache;
        _currencyRepository = currencyRepository;
        _userRepository = userRepository;
        _userItemRepository = userItemRepository;
        _userGiftRepository = userGiftRepository;
        _furnitureDefinitions = furnitureDefinitions;
        _subscriptionManager = subscriptionManager;
        _sessionManager = sessionManager;
        _achievementService = achievementService;
        _history = history;
    }

    public void Parse(ClientPacket packet)
    {
        _pageId = packet.PopInt32();
        _offerId = packet.PopInt32();
        _extraData = packet.PopString();
        _receiverName = packet.PopString();
        _giftMessage = packet.PopString();
        _boxId = packet.PopInt32();
        _colourValue = packet.PopInt32();
        _ribbonId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        if (_hotel.IsShuttingDown)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.PurchasesOffShutdown)).Compose());
            return;
        }

        CatalogPageSummary? page = _catalogCache.Pages.FirstOrDefault(p => p.Id == _pageId);
        if (page is null || !page.Enabled || page.ComingSoon)
        {
            session.Send(new PurchaseErrorMessageEvent(PurchaseErrorReason.OfferNotFound).Compose());
            return;
        }

        if (page.MinRank > account.Rank)
        {
            session.Send(new PurchaseNotAllowedMessageEvent(PurchaseNotAllowedReason.Unknown).Compose());
            return;
        }

        CatalogOffer? offer = _catalogCache.GetOffers(_pageId).FirstOrDefault(o => o.Id == _offerId);
        if (offer is null)
        {
            session.Send(new PurchaseErrorMessageEvent(PurchaseErrorReason.OfferNotFound).Compose());
            return;
        }

        if (!offer.IsActuallyGiftable)
        {
            session.Send(new PurchaseErrorMessageEvent(PurchaseErrorReason.OfferNotPurchasable).Compose());
            return;
        }

        if (offer.ClubOnly && !await _subscriptionManager.IsClubMemberAsync(account.Id, cancellationToken))
        {
            session.Send(new PurchaseNotAllowedMessageEvent(PurchaseNotAllowedReason.HabboClubRequired).Compose());
            return;
        }

        UserAccount? receiver = await _userRepository.GetByUsernameAsync(_receiverName, cancellationToken);
        if (receiver is null)
        {
            session.Send(new GiftReceiverNotFoundEvent().Compose());
            return;
        }

        CatalogOfferProduct product = offer.Products[0];
        var contents = new List<GiftContent>();

        if (product.ProductType == CatalogProductType.Subscription)
        {
            if (!int.TryParse(product.ExtraData, out int months) || months <= 0)
            {
                session.Send(new PurchaseErrorMessageEvent(PurchaseErrorReason.OfferNotPurchasable).Compose());
                return;
            }

            for (int i = 0; i < product.Quantity; i++)
            {
                contents.Add(new GiftContent(GiftContentType.Subscription, "habbo_club", months.ToString()));
            }
        }
        else
        {
            if (product.FurnitureDefId is not { } furniDefId)
            {
                session.Send(new PurchaseErrorMessageEvent(PurchaseErrorReason.OfferNotPurchasable).Compose());
                return;
            }

            FurnitureDefinition? productDef = _furnitureDefinitions.GetById(furniDefId);
            string productExtraData = productDef?.LogicClass == FurnitureLogicClass.Trophy
                ? _extraData
                : product.ExtraData ?? string.Empty;

            for (int i = 0; i < product.Quantity; i++)
            {
                contents.Add(new GiftContent(GiftContentType.Furni, furniDefId.ToString(), productExtraData));
            }
        }

        GiftWrapConfig? giftWrapConfig = _catalogCache.GiftWrapConfig;
        if (giftWrapConfig is null)
        {
            session.Send(new PurchaseErrorMessageEvent(PurchaseErrorReason.OfferNotPurchasable).Compose());
            return;
        }

        int totalPriceCredits = offer.PriceCredits + giftWrapConfig.WrapPriceCredits;

        long credits = await _currencyRepository.GetBalanceAsync(account.Id, CurrencyType.Credits, cancellationToken);
        long activityPoints = await _currencyRepository.GetBalanceAsync(account.Id, CurrencyType.ActivityPoints, cancellationToken);
        if (credits < totalPriceCredits || activityPoints < offer.PriceActivityPoints)
        {
            session.Send(new NotEnoughBalanceMessageEvent(credits < totalPriceCredits, activityPoints < offer.PriceActivityPoints).Compose());
            return;
        }

        int wrapperFurniDefId;
        int? wrapExtras;

        if (_boxId == 0)
        {
            string randomClassname = FreeWrapClassnames[Random.Shared.Next(FreeWrapClassnames.Length)];
            FurnitureDefinition? freeWrapDef = _furnitureDefinitions.GetByClassname(randomClassname);
            if (freeWrapDef is null)
            {
                session.Send(new PurchaseErrorMessageEvent(PurchaseErrorReason.OfferNotPurchasable).Compose());
                return;
            }

            wrapperFurniDefId = freeWrapDef.Id;
            wrapExtras = null;
        }
        else
        {
            GiftWrapBox? box = giftWrapConfig.Boxes.FirstOrDefault(b => b.FurnidataRef == _boxId);
            if (box is null)
            {
                session.Send(new PurchaseErrorMessageEvent(PurchaseErrorReason.OfferNotPurchasable).Compose());
                return;
            }

            wrapperFurniDefId = box.FurniDefId;
            wrapExtras = (_colourValue * 1000) + _ribbonId;
        }

        if (totalPriceCredits > 0)
        {
            await _currencyRepository.SetBalanceAsync(account.Id, CurrencyType.Credits, credits - totalPriceCredits, cancellationToken);
        }

        if (offer.PriceActivityPoints > 0)
        {
            await _currencyRepository.SetBalanceAsync(account.Id, CurrencyType.ActivityPoints, activityPoints - offer.PriceActivityPoints, cancellationToken);
        }

        string wrapperExtraData = string.IsNullOrEmpty(_giftMessage) ? string.Empty : "!" + _giftMessage;
        int wrapperItemId = await _userItemRepository.CreateInInventoryAsync(
            receiver.Id, wrapperFurniDefId, wrapperExtraData, seen: false, wrapExtras, cancellationToken);
        await _userGiftRepository.CreateAsync(wrapperItemId, account.Id, contents, cancellationToken);

        if (receiver.Id != account.Id)
        {
            await _achievementService.IncrementAsync(account.Id, "ACH_GiftGiver", cancellationToken: cancellationToken);
        }

        _history.Record(new CatalogPurchaseRecord(
            DateTime.UtcNow, account.Id, _pageId, offer.Id, totalPriceCredits, offer.PriceActivityPoints, receiver.Id, session.RemoteAddress));
        session.Send(new PurchaseOKMessageEvent(offer).Compose());

        if (totalPriceCredits > 0)
        {
            session.Send(new CreditBalanceEvent(credits - totalPriceCredits).Compose());
        }

        if (offer.PriceActivityPoints > 0)
        {
            session.Send(new HabboActivityPointNotificationMessageEvent(activityPoints - offer.PriceActivityPoints).Compose());
        }

        if (_sessionManager.TryGetByUserId(receiver.Id, out ClientSession? receiverSession))
        {
            UserItem? wrapperItem = await _userItemRepository.GetByIdAsync(wrapperItemId, cancellationToken);
            if (wrapperItem is not null)
            {
                receiverSession!.EnqueueSend(new FurniListInsertEvent(wrapperItem).Compose());
            }
        }
    }
}
