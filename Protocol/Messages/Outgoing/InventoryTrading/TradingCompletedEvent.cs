namespace DragonEmu.Protocol.Messages.Outgoing.InventoryTrading;

public sealed class TradingCompletedEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.InventoryTrading.TradingCompletedEvent);
}
