namespace DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;

public sealed class FurniListRemoveEvent : IMessageEvent
{
    private readonly int _itemId;

    public FurniListRemoveEvent(int itemId)
    {
        _itemId = itemId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryFurni.FurniListRemoveEvent);
        packet.AppendInt32(_itemId);
        return packet;
    }
}
