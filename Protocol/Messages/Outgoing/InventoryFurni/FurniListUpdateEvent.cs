namespace DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;

public sealed class FurniListUpdateEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.InventoryFurni.FurniListUpdateEvent);
}
