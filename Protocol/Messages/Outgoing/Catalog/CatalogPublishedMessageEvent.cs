namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public sealed class CatalogPublishedMessageEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.Catalog.CatalogPublishedMessageEvent);
}
