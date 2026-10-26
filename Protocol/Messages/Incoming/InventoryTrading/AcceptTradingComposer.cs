using DragonEmu.Game.Trading;

namespace DragonEmu.Protocol.Messages.Incoming.InventoryTrading;

public sealed class AcceptTradingComposer : IMessageComposer
{
    private readonly TradingService _trading;

    public AcceptTradingComposer(TradingService trading)
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
            _trading.Accept(session, account);
        }

        return ValueTask.CompletedTask;
    }
}
