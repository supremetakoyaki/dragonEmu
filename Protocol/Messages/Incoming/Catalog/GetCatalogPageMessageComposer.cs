using DragonEmu.Domain;
using DragonEmu.Game.Catalog;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;

namespace DragonEmu.Protocol.Messages.Incoming.Catalog;

public sealed class GetCatalogPageMessageComposer : IMessageComposer
{
    private readonly CatalogCache _catalogCache;

    private int _pageId;

    public GetCatalogPageMessageComposer(CatalogCache catalogCache)
    {
        _catalogCache = catalogCache;
    }

    public void Parse(ClientPacket packet)
    {
        _pageId = packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return ValueTask.CompletedTask;
        }

        CatalogPageSummary? page = _catalogCache.Pages.FirstOrDefault(p => p.Id == _pageId);

        if (page is null || !page.Enabled || page.ComingSoon || page.MinRank > account.Rank)
        {
            return ValueTask.CompletedTask;
        }

        CatalogPageBody? body = _catalogCache.GetBody(_pageId);
        if (body is null)
        {
            return ValueTask.CompletedTask;
        }

        IReadOnlyList<CatalogOffer> offers = _catalogCache.GetOffers(_pageId);

        session.Send(new CatalogPageMessageEvent(body, offers).Compose());
        return ValueTask.CompletedTask;
    }
}
