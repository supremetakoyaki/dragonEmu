using DragonEmu.Game.Trading;

namespace DragonEmu.Protocol.Messages.Incoming.InventoryTrading;

public sealed class AddItemToTradeComposer : IMessageComposer
{
    private readonly TradingService _trading;

    private int _itemId;

    public AddItemToTradeComposer(TradingService trading)
    {
        _trading = trading;
    }

    public void Parse(ClientPacket packet)
    {
        _itemId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _trading.OfferAsync(session, account, _itemId, cancellationToken);
        }
    }
}
