using DragonEmu.Game.Trading;

namespace DragonEmu.Protocol.Messages.Incoming.InventoryTrading;

public sealed class UnacceptTradingComposer : IMessageComposer
{
    private readonly TradingService _trading;

    public UnacceptTradingComposer(TradingService trading)
    {
        _trading = trading;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            _trading.Unaccept(session, account);
        }

        return ValueTask.CompletedTask;
    }
}
