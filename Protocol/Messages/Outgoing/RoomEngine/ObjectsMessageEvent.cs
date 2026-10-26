using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class ObjectsMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<UserItem> _items;

    public ObjectsMessageEvent(IReadOnlyList<UserItem> items)
    {
        _items = items;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.ObjectsMessageEvent);

        packet.AppendInt32(_items.Count);
        foreach (UserItem item in _items)
        {
            RoomFloorItemWireFormat.AppendItem(packet, item);
        }

        return packet;
    }
}
