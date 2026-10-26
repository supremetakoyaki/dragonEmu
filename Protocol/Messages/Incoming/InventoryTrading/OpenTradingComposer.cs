using DragonEmu.Game.Trading;

namespace DragonEmu.Protocol.Messages.Incoming.InventoryTrading;

public sealed class OpenTradingComposer : IMessageComposer
{
    private readonly TradingService _trading;

    private int _partnerVirtualId;

    public OpenTradingComposer(TradingService trading)
    {
        _trading = trading;
    }

    public void Parse(ClientPacket packet)
    {
        _partnerVirtualId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _trading.OpenAsync(session, account, _partnerVirtualId, cancellationToken);
        }
    }
}
