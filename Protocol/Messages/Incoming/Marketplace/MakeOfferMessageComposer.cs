using DragonEmu.Game.Marketplace;

namespace DragonEmu.Protocol.Messages.Incoming.Marketplace;

public sealed class MakeOfferMessageComposer : IMessageComposer
{
    private readonly MarketplaceService _marketplace;

    private int _askingPrice;
    private int _furniType;
    private int _itemId;

    public MakeOfferMessageComposer(MarketplaceService marketplace)
    {
        _marketplace = marketplace;
    }

    public void Parse(ClientPacket packet)
    {
        _askingPrice = packet.PopInt32();
        _furniType = packet.PopInt32();
        _itemId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _marketplace.MakeOfferAsync(session, account, _askingPrice, _itemId, cancellationToken);
        }
    }
}
