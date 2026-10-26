using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class ItemsMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<UserItem> _items;

    public ItemsMessageEvent(IReadOnlyList<UserItem> items)
    {
        _items = items;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.ItemsMessageEvent);

        packet.AppendInt32(_items.Count);
        foreach (UserItem item in _items)
        {
            RoomWallItemWireFormat.AppendItem(packet, item);
        }

        return packet;
    }
}
