using DragonEmu.Game.Marketplace;

namespace DragonEmu.Protocol.Messages.Incoming.Marketplace;

public sealed class GetMarketplaceItemStatsComposer : IMessageComposer
{
    private readonly MarketplaceService _marketplace;

    private int _furniType;
    private int _sprite;

    public GetMarketplaceItemStatsComposer(MarketplaceService marketplace)
    {
        _marketplace = marketplace;
    }

    public void Parse(ClientPacket packet)
    {
        _furniType = packet.PopInt32();
        _sprite = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _marketplace.SendItemStatsAsync(session, account, _furniType, _sprite, cancellationToken);
        }
    }
}
