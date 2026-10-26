using DragonEmu.Game.Marketplace;

namespace DragonEmu.Protocol.Messages.Incoming.Marketplace;

public sealed class RedeemOfferCreditsMessageComposer : IMessageComposer
{
    private readonly MarketplaceService _marketplace;

    public RedeemOfferCreditsMessageComposer(MarketplaceService marketplace)
    {
        _marketplace = marketplace;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _marketplace.RedeemAsync(session, account, cancellationToken);
        }
    }
}
