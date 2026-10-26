using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class ObjectUpdateMessageEvent : IMessageEvent
{
    private readonly UserItem _item;

    public ObjectUpdateMessageEvent(UserItem item)
    {
        _item = item;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.ObjectUpdateMessageEvent);
        RoomFloorItemWireFormat.AppendItem(packet, _item);
        return packet;
    }
}
