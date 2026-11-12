using DragonEmu.Domain;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Catalog;

public sealed class CatalogCache : IHostedService
{
    private readonly ICatalogPageRepository _catalogPageRepository;
    private readonly ICatalogPageBodyRepository _catalogPageBodyRepository;
    private readonly ICatalogOfferRepository _catalogOfferRepository;
    private readonly IClubGiftRepository _clubGiftRepository;
    private readonly IClubOfferRepository _clubOfferRepository;
    private readonly IGiftWrapConfigRepository _giftWrapConfigRepository;
    private readonly ILogger<CatalogCache> _logger;

    private volatile Snapshot _snapshot = Snapshot.Empty;

    public CatalogCache(
        ICatalogPageRepository catalogPageRepository,
        ICatalogPageBodyRepository catalogPageBodyRepository,
        ICatalogOfferRepository catalogOfferRepository,
        IClubGiftRepository clubGiftRepository,
        IClubOfferRepository clubOfferRepository,
        IGiftWrapConfigRepository giftWrapConfigRepository,
        ILogger<CatalogCache> logger)
    {
        _catalogPageRepository = catalogPageRepository;
        _catalogPageBodyRepository = catalogPageBodyRepository;
        _catalogOfferRepository = catalogOfferRepository;
        _clubGiftRepository = clubGiftRepository;
        _clubOfferRepository = clubOfferRepository;
        _giftWrapConfigRepository = giftWrapConfigRepository;
        _logger = logger;
    }

    public IReadOnlyList<CatalogPageSummary> Pages => _snapshot.Pages;

    public CatalogPageBody? GetBody(int pageId) => _snapshot.Bodies.GetValueOrDefault(pageId);

    public IReadOnlyList<CatalogOffer> GetOffers(int pageId) => _snapshot.Offers.GetValueOrDefault(pageId, []);

    public IReadOnlyList<ClubGiftDefinition> ClubGifts => _snapshot.ClubGifts;

    public IReadOnlyList<ClubOffer> ClubOffers => _snapshot.ClubOffers;

    public ClubOffer? GetClubOffer(int offerId) => _snapshot.ClubOffers.FirstOrDefault(o => o.OfferId == offerId);

    public GiftWrapConfig? GiftWrapConfig => _snapshot.GiftWrapConfig;

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CatalogPageSummary> pages = await _catalogPageRepository.GetAllAsync(cancellationToken);

        var bodies = new Dictionary<int, CatalogPageBody>();
        var offers = new Dictionary<int, IReadOnlyList<CatalogOffer>>();

        foreach (CatalogPageSummary page in pages)
        {
            CatalogPageBody? body = await _catalogPageBodyRepository.GetByPageIdAsync(page.Id, cancellationToken);
            if (body is not null)
            {
                bodies[page.Id] = body;
            }

            offers[page.Id] = await _catalogOfferRepository.GetByPageIdAsync(page.Id, cancellationToken);
        }

        IReadOnlyList<ClubGiftDefinition> clubGifts = await _clubGiftRepository.GetAllAsync(cancellationToken);
        GiftWrapConfig? giftWrapConfig = await _giftWrapConfigRepository.GetAsync(cancellationToken);

        var clubOffers = new List<ClubOffer>();
        foreach (ClubOfferDefinition definition in await _clubOfferRepository.GetAllAsync(cancellationToken))
        {
            CatalogOffer? offer = offers.Values.SelectMany(list => list).FirstOrDefault(o => o.Id == definition.OfferId);
            if (offer is null)
            {
                _logger.LogWarning("Club offer {OfferId} has no catalog offer - left out.", definition.OfferId);
                continue;
            }

            clubOffers.Add(new ClubOffer(offer, definition.PeriodDays, definition.Vip, definition.Upgrade));
        }

        _snapshot = new Snapshot(pages, bodies, offers, clubGifts, clubOffers, giftWrapConfig);
        _logger.LogInformation(
            "Catalog cache loaded: {PageCount} pages, {BodyCount} bodies, {ClubGiftCount} club gifts, {ClubOfferCount} club offers.", pages.Count, bodies.Count, clubGifts.Count, clubOffers.Count);
    }

    public Task StartAsync(CancellationToken cancellationToken) => ReloadAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private sealed record Snapshot(
        IReadOnlyList<CatalogPageSummary> Pages,
        IReadOnlyDictionary<int, CatalogPageBody> Bodies,
        IReadOnlyDictionary<int, IReadOnlyList<CatalogOffer>> Offers,
        IReadOnlyList<ClubGiftDefinition> ClubGifts,
        IReadOnlyList<ClubOffer> ClubOffers,
        GiftWrapConfig? GiftWrapConfig)
    {
        public static readonly Snapshot Empty = new([], new Dictionary<int, CatalogPageBody>(), new Dictionary<int, IReadOnlyList<CatalogOffer>>(), [], [], null);
    }
}
