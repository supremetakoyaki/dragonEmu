namespace DragonEmu.Protocol.Messages.Outgoing.InventoryTrading;

public sealed class TradingYouAreNotAllowedEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.InventoryTrading.TradingYouAreNotAllowedEvent);
}
