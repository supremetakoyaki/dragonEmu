using DragonEmu.Domain;
using DragonEmu.Game.Catalog;
using DragonEmu.Game.Fuses;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;

namespace DragonEmu.Protocol.Messages.Incoming.Catalog;

public sealed class GetCatalogPageMessageComposer : IMessageComposer
{
    private readonly CatalogCache _catalogCache;
    private readonly FuseService _fuses;

    private int _pageId;
    private int _offerId = -1;

    public GetCatalogPageMessageComposer(CatalogCache catalogCache, FuseService fuses)
    {
        _catalogCache = catalogCache;
        _fuses = fuses;
    }

    public void Parse(ClientPacket packet)
    {
        _pageId = packet.PopInt32();

        if (packet.RemainingLength > 0)
        {
            _offerId = packet.PopInt32();
        }
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        CatalogPageSummary? page = _catalogCache.Pages.FirstOrDefault(p => p.Id == _pageId);

        if (page is null || !page.Enabled || page.MinRank > account.Rank)
        {
            return;
        }

        if (page.ComingSoon && !await _fuses.HoldsAsync(account, RoomRightsService.AdministratorAccessFuse, cancellationToken: cancellationToken))
        {
            return;
        }

        CatalogPageBody? body = _catalogCache.GetBody(_pageId);
        if (body is null)
        {
            return;
        }

        IReadOnlyList<CatalogOffer> offers = _catalogCache.GetOffers(_pageId);

        session.Send(new CatalogPageMessageEvent(body, offers, session.Language, _offerId).Compose());
    }
}
