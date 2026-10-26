using DragonEmu.Game.Marketplace;

namespace DragonEmu.Protocol.Messages.Incoming.Marketplace;

public sealed class GetMarketplaceConfigurationMessageComposer : IMessageComposer
{
    private readonly MarketplaceService _marketplace;

    public GetMarketplaceConfigurationMessageComposer(MarketplaceService marketplace)
    {
        _marketplace = marketplace;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        await _marketplace.SendConfigAsync(session, cancellationToken);
    }
}
