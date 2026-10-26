using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;
using DragonEmu.Domain;
using DragonEmu.Game.Catalog;
using DragonEmu.Game.Furniture;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;

namespace DragonEmu.Protocol.Messages.Incoming.Catalog;

public sealed class SelectClubGiftMessageComposer : IMessageComposer
{
    private readonly CatalogCache _catalogCache;
    private readonly IUserRepository _userRepository;
    private readonly IUserItemRepository _userItemRepository;
    private readonly FurnitureDefinitionCache _furnitureDefinitions;
    private readonly TeleportPairGrantService _teleportPairGrantService;

    private readonly HotelStatus _hotel;
    private readonly MessageTexts _texts;

    private string _offerCode = string.Empty;

    public SelectClubGiftMessageComposer(
        CatalogCache catalogCache, IUserRepository userRepository, IUserItemRepository userItemRepository,
        FurnitureDefinitionCache furnitureDefinitions, TeleportPairGrantService teleportPairGrantService, HotelStatus hotel, MessageTexts texts)
    {
        _texts = texts;
        _hotel = hotel;
        _catalogCache = catalogCache;
        _userRepository = userRepository;
        _userItemRepository = userItemRepository;
        _furnitureDefinitions = furnitureDefinitions;
        _teleportPairGrantService = teleportPairGrantService;
    }

    public void Parse(ClientPacket packet)
    {
        _offerCode = packet.PopString();
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

        ClubGiftDefinition? gift = _catalogCache.ClubGifts.FirstOrDefault(g => g.OfferCode == _offerCode);
        if (gift is null || gift.Product.FurnitureDefId is not { } furniDefId)
        {
            return;
        }

        FurnitureDefinition? definition = _furnitureDefinitions.GetById(furniDefId);
        if (definition is null)
        {
            return;
        }

        (_, int accumulatedMonths, _) = await _userRepository.GetClubStatusAsync(account.Id, cancellationToken);
        if (accumulatedMonths < gift.MonthsRequired)
        {
            return;
        }

        if (!await _userRepository.TryClaimClubGiftAsync(account.Id, cancellationToken))
        {
            return;
        }

        IReadOnlyList<int> itemIds = await _teleportPairGrantService.GrantAsync(
            account.Id, definition, gift.Product.ExtraData, seen: true, gift.Product.Quantity, cancellationToken);

        foreach (UserItem item in await _userItemRepository.GetByIdsAsync([.. itemIds], cancellationToken))
        {
            session.Send(new FurniListInsertEvent(item).Compose());
        }

        session.Send(new ClubGiftSelectedEvent(gift.OfferCode, gift.Product).Compose());
    }
}
