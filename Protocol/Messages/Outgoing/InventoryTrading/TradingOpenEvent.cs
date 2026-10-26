namespace DragonEmu.Protocol.Messages.Outgoing.InventoryTrading;

public sealed class TradingOpenEvent : IMessageEvent
{
    private readonly int _userId;
    private readonly bool _canTrade;
    private readonly int _otherUserId;
    private readonly bool _otherCanTrade;

    public TradingOpenEvent(int userId, bool canTrade, int otherUserId, bool otherCanTrade)
    {
        _userId = userId;
        _canTrade = canTrade;
        _otherUserId = otherUserId;
        _otherCanTrade = otherCanTrade;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryTrading.TradingOpenEvent);
        packet.AppendInt32(_userId);
        packet.AppendInt32(_canTrade ? 1 : 0);
        packet.AppendInt32(_otherUserId);
        packet.AppendInt32(_otherCanTrade ? 1 : 0);
        return packet;
    }
}
