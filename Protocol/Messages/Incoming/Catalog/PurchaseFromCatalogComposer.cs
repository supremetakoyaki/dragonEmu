using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Fuses;
using DragonEmu.Domain;
using DragonEmu.Game.Avatars;
using DragonEmu.Game.Catalog;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.History;
using DragonEmu.Game.Pets;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Subscriptions;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;
using DragonEmu.Protocol.Messages.Outgoing.Handshake;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.InventoryPurse;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;
using DragonEmu.Protocol.Messages.Outgoing.Users;
using DragonEmu.Game.Settings;

namespace DragonEmu.Protocol.Messages.Incoming.Catalog;

public sealed class PurchaseFromCatalogComposer : IMessageComposer
{
    private readonly CatalogCache _catalogCache;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUserRepository _userRepository;
    private readonly FuseService _fuses;
    private readonly IUserItemRepository _userItemRepository;
    private readonly FurnitureDefinitionCache _furnitureDefinitions;
    private readonly SubscriptionManager _subscriptionManager;
    private readonly TeleportPairGrantService _teleportPairGrantService;
    private readonly AvatarEffectService _avatarEffectService;
    private readonly HistoryLog _history;
    private readonly HotelClock _clock;
    private readonly PetService _pets;
    private readonly HotelStatus _hotel;
    private readonly MessageTexts _texts;

    private int _pageId;
    private int _offerId;
    private string _extraData = string.Empty;

    public PurchaseFromCatalogComposer(
        CatalogCache catalogCache,
        ICurrencyRepository currencyRepository,
        IUserRepository userRepository,
        FuseService fuses,
        IUserItemRepository userItemRepository,
        FurnitureDefinitionCache furnitureDefinitions,
        SubscriptionManager subscriptionManager,
        TeleportPairGrantService teleportPairGrantService,
        AvatarEffectService avatarEffectService,
        HistoryLog history,
        HotelClock clock,
        PetService pets,
        HotelStatus hotel, MessageTexts texts)
    {
        _texts = texts;
        _hotel = hotel;
        _pets = pets;
        _clock = clock;
        _catalogCache = catalogCache;
        _currencyRepository = currencyRepository;
        _userRepository = userRepository;
        _fuses = fuses;
        _userItemRepository = userItemRepository;
        _furnitureDefinitions = furnitureDefinitions;
        _subscriptionManager = subscriptionManager;
        _teleportPairGrantService = teleportPairGrantService;
        _avatarEffectService = avatarEffectService;
        _history = history;
    }

    public void Parse(ClientPacket packet)
    {
        _pageId = packet.PopInt32();
        _offerId = packet.PopInt32();
        _extraData = packet.PopString();
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

        if (offer.ClubOnly && !await _subscriptionManager.IsClubMemberAsync(account.Id, cancellationToken))
        {
            session.Send(new PurchaseNotAllowedMessageEvent(PurchaseNotAllowedReason.HabboClubRequired).Compose());
            return;
        }

        var subscriptionMonths = new List<int>();
        var furnitureToCreate = new List<(FurnitureDefinition Definition, string? ExtraData, int Count, int RentalMinutes)>();
        var effectsToGrant = new List<(int EffectId, int Duration, int Count)>();

        PetPurchase? pet = null;
        if (PetOffer.TypeOf(offer) is { } petType)
        {
            pet = _pets.TryReadPurchase(petType, _extraData);
            if (pet is null)
            {
                session.Send(new PurchaseErrorMessageEvent(PurchaseErrorReason.OfferNotPurchasable).Compose());
                return;
            }
        }

        foreach (CatalogOfferProduct product in pet is null ? offer.Products : [])
        {
            switch (product.ProductType)
            {
                case CatalogProductType.Subscription:
                    if (!int.TryParse(product.ExtraData, out int months) || months <= 0)
                    {
                        session.Send(new PurchaseErrorMessageEvent(PurchaseErrorReason.OfferNotPurchasable).Compose());
                        return;
                    }

                    subscriptionMonths.Add(months);
                    break;

                case CatalogProductType.Floor:
                case CatalogProductType.Wall:
                    if (product.FurnitureDefId is not { } furniDefId)
                    {
                        session.Send(new PurchaseErrorMessageEvent(PurchaseErrorReason.OfferNotPurchasable).Compose());
                        return;
                    }

                    FurnitureDefinition? productDef = _furnitureDefinitions.GetById(furniDefId);
                    if (productDef is null)
                    {
                        session.Send(new PurchaseErrorMessageEvent(PurchaseErrorReason.OfferNotPurchasable).Compose());
                        return;
                    }

                    bool isRoomPaint = RoomPaintService.PaintTypeFor(productDef.Classname) is not null;

                    string? extra;
                    if (isRoomPaint && !string.IsNullOrEmpty(_extraData))
                    {
                        extra = _extraData;
                    }
                    else if (productDef.LogicClass == FurnitureLogicClass.Trophy)
                    {
                        string message = _extraData.Replace("\t", " ").Replace("\n", " ");
                        extra = $"{account.Username}\t{_clock.TodayText}\t{message}";
                    }
                    else
                    {
                        extra = product.ExtraData;
                    }

                    furnitureToCreate.Add((productDef, extra, product.Quantity, product.RentalDuration));
                    break;

                case CatalogProductType.Effect:
                    if (!int.TryParse(product.ExtraData, out int effectId) || effectId <= 0 || product.RentalDuration <= 0)
                    {
                        session.Send(new PurchaseErrorMessageEvent(PurchaseErrorReason.OfferNotPurchasable).Compose());
                        return;
                    }

                    effectsToGrant.Add((effectId, product.RentalDuration, product.Quantity));
                    break;

                default:
                    session.Send(new PurchaseErrorMessageEvent(PurchaseErrorReason.OfferNotPurchasable).Compose());
                    return;
            }
        }

        long credits = await _currencyRepository.GetBalanceAsync(account.Id, CurrencyType.Credits, cancellationToken);
        long activityPoints = await _currencyRepository.GetBalanceAsync(account.Id, CurrencyType.ActivityPoints, cancellationToken);
        if (credits < offer.PriceCredits || activityPoints < offer.PriceActivityPoints)
        {
            session.Send(new NotEnoughBalanceMessageEvent(credits < offer.PriceCredits, activityPoints < offer.PriceActivityPoints).Compose());
            return;
        }

        if (offer.PriceCredits > 0)
        {
            await _currencyRepository.SetBalanceAsync(account.Id, CurrencyType.Credits, credits - offer.PriceCredits, cancellationToken);
        }

        if (offer.PriceActivityPoints > 0)
        {
            await _currencyRepository.SetBalanceAsync(account.Id, CurrencyType.ActivityPoints, activityPoints - offer.PriceActivityPoints, cancellationToken);
        }

        SubscriptionGrantResult? lastGrant = null;
        foreach (int months in subscriptionMonths)
        {
            lastGrant = await _subscriptionManager.GrantMonthsAsync(account.Id, months, cancellationToken);
        }

        var createdItemIds = new List<int>();
        foreach ((FurnitureDefinition definition, string? extraData, int count, int rentalMinutes) in furnitureToCreate)
        {
            IReadOnlyList<int> created = await _teleportPairGrantService.GrantAsync(account.Id, definition, extraData, seen: true, count, cancellationToken);
            createdItemIds.AddRange(created);

            if (rentalMinutes > 0)
            {
                await _userItemRepository.SetRentalExpiryAsync(created, DateTime.UtcNow.ToWholeSecond().AddMinutes(rentalMinutes), cancellationToken);
            }
        }

        foreach ((int effectId, int duration, int count) in effectsToGrant)
        {
            await _avatarEffectService.GrantAsync(session, account.Id, effectId, duration, count, cancellationToken);
        }

        _history.Record(new CatalogPurchaseRecord(
            DateTime.UtcNow, account.Id, _pageId, offer.Id, offer.PriceCredits, offer.PriceActivityPoints, GiftReceiverId: null, session.RemoteAddress));
        session.Send(new PurchaseOKMessageEvent(offer).Compose());

        if (pet is not null)
        {
            await _pets.GiveAsync(session, account, pet, cancellationToken);
        }

        foreach (UserItem item in await _userItemRepository.GetByIdsAsync([.. createdItemIds], cancellationToken))
        {
            session.Send(new FurniListInsertEvent(item).Compose());
        }

        if (offer.PriceCredits > 0)
        {
            session.Send(new CreditBalanceEvent(credits - offer.PriceCredits).Compose());
        }

        if (offer.PriceActivityPoints > 0)
        {
            session.Send(new HabboActivityPointNotificationMessageEvent(activityPoints - offer.PriceActivityPoints).Compose());
        }

        if (lastGrant is { } grant)
        {
            session.Send(ScrSendUserInfoEvent.HabboClubJustSubscribed(grant.ClubExpiresAt).Compose());

            await _fuses.SendHeldAsync(session, account, cancellationToken);
        }
    }
}
