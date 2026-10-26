using DragonEmu.Domain;
using DragonEmu.Game.Catalog;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;

namespace DragonEmu.Protocol.Messages.Incoming.Catalog;

public sealed class GetIsOfferGiftableMessageComposer : IMessageComposer
{
    private readonly CatalogCache _catalogCache;

    private int _offerId;

    public GetIsOfferGiftableMessageComposer(CatalogCache catalogCache)
    {
        _catalogCache = catalogCache;
    }

    public void Parse(ClientPacket packet)
    {
        _offerId = packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is null)
        {
            return ValueTask.CompletedTask;
        }

        CatalogOffer? offer = _catalogCache.Pages
            .SelectMany(p => _catalogCache.GetOffers(p.Id))
            .FirstOrDefault(o => o.Id == _offerId);

        session.Send(new IsOfferGiftableMessageEvent(_offerId, offer?.IsActuallyGiftable ?? false).Compose());
        return ValueTask.CompletedTask;
    }
}
