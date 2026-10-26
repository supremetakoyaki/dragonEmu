using DragonEmu.Game.Marketplace;

namespace DragonEmu.Protocol.Messages.Incoming.Marketplace;

public sealed class GetOffersMessageComposer : IMessageComposer
{
    private readonly MarketplaceService _marketplace;

    private int _minPrice;
    private int _maxPrice;
    private string _name = string.Empty;
    private int _sort;

    public GetOffersMessageComposer(MarketplaceService marketplace)
    {
        _marketplace = marketplace;
    }

    public void Parse(ClientPacket packet)
    {
        _minPrice = packet.PopInt32();
        _maxPrice = packet.PopInt32();
        _name = packet.PopString();
        _sort = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _marketplace.SearchAsync(session, account, _minPrice, _maxPrice, _name, _sort, cancellationToken);
        }
    }
}
