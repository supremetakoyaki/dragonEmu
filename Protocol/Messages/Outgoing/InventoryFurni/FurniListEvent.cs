using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;

public sealed class FurniListEvent : IMessageEvent
{
    private readonly IReadOnlyList<UserItem> _items;

    public FurniListEvent(IReadOnlyList<UserItem> items)
    {
        _items = items;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryFurni.FurniListEvent);

        packet.AppendInt32(_items.Count);
        foreach (UserItem item in _items)
        {
            UserItemWireFormat.AppendItem(packet, item);
        }

        return packet;
    }
}
