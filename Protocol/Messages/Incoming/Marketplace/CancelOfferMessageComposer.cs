using DragonEmu.Game.Marketplace;

namespace DragonEmu.Protocol.Messages.Incoming.Marketplace;

public sealed class CancelOfferMessageComposer : IMessageComposer
{
    private readonly MarketplaceService _marketplace;

    private int _offerId;

    public CancelOfferMessageComposer(MarketplaceService marketplace)
    {
        _marketplace = marketplace;
    }

    public void Parse(ClientPacket packet)
    {
        _offerId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _marketplace.CancelAsync(session, account, _offerId, cancellationToken);
        }
    }
}
