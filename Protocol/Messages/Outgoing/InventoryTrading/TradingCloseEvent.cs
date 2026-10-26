namespace DragonEmu.Protocol.Messages.Outgoing.InventoryTrading;

public sealed class TradingCloseEvent : IMessageEvent
{
    private readonly int _userId;

    public TradingCloseEvent(int userId)
    {
        _userId = userId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryTrading.TradingCloseEvent);
        packet.AppendInt32(_userId);
        return packet;
    }
}
