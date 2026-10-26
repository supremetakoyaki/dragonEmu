using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class PublicRoomObjectsMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<PublicRoomFurnitureItem> _items;

    public PublicRoomObjectsMessageEvent(IReadOnlyList<PublicRoomFurnitureItem> items)
    {
        _items = items;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.PublicRoomObjectsMessageEvent);
        packet.AppendInt32(_items.Count);

        foreach (PublicRoomFurnitureItem item in _items)
        {
            packet.AppendInt32(0);
            packet.AppendString(item.Classname);
            packet.AppendString(item.Classname);
            packet.AppendInt32(item.X);
            packet.AppendInt32(item.Y);
            packet.AppendInt32((int)item.Z);
            packet.AppendInt32(item.Rotation);
        }

        return packet;
    }
}
