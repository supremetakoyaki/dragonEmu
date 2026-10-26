namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public sealed class GiftReceiverNotFoundEvent : IMessageEvent
{
    public ServerPacket Compose()
    {
        return new ServerPacket(OutgoingOpcodes.Catalog.GiftReceiverNotFoundEvent);
    }
}
