namespace DragonEmu.Protocol.Messages.Outgoing.InventoryTrading;

public sealed class TradingAcceptEvent : IMessageEvent
{
    private readonly int _userId;
    private readonly bool _accepted;

    public TradingAcceptEvent(int userId, bool accepted)
    {
        _userId = userId;
        _accepted = accepted;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryTrading.TradingAcceptEvent);
        packet.AppendInt32(_userId);
        packet.AppendInt32(_accepted ? 1 : 0);
        return packet;
    }
}
