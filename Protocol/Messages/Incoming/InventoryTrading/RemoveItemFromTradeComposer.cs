using DragonEmu.Game.Trading;

namespace DragonEmu.Protocol.Messages.Incoming.InventoryTrading;

public sealed class RemoveItemFromTradeComposer : IMessageComposer
{
    private readonly TradingService _trading;

    private int _itemId;

    public RemoveItemFromTradeComposer(TradingService trading)
    {
        _trading = trading;
    }

    public void Parse(ClientPacket packet)
    {
        _itemId = packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            _trading.TakeBack(session, account, _itemId);
        }

        return ValueTask.CompletedTask;
    }
}
