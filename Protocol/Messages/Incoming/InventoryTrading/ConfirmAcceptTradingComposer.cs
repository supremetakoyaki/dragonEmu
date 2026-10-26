using DragonEmu.Game.Trading;

namespace DragonEmu.Protocol.Messages.Incoming.InventoryTrading;

public sealed class ConfirmAcceptTradingComposer : IMessageComposer
{
    private readonly TradingService _trading;

    public ConfirmAcceptTradingComposer(TradingService trading)
    {
        _trading = trading;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _trading.ConfirmAsync(session, account);
        }
    }
}
