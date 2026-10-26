namespace DragonEmu.Protocol.Messages.Outgoing.InventoryTrading;

public sealed class TradingAlreadyOpenEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.InventoryTrading.TradingAlreadyOpenEvent);
}
