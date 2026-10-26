namespace DragonEmu.Protocol.Messages.Outgoing.InventoryTrading;

public sealed class TradingConfirmationEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.InventoryTrading.TradingConfirmationEvent);
}
