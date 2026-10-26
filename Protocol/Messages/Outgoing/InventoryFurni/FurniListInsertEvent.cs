using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;

public sealed class FurniListInsertEvent : IMessageEvent
{
    private readonly UserItem _item;

    public FurniListInsertEvent(UserItem item)
    {
        _item = item;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryFurni.FurniListInsertEvent);
        UserItemWireFormat.AppendItem(packet, _item);
        return packet;
    }
}
