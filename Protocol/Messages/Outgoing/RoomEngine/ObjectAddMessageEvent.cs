using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class ObjectAddMessageEvent : IMessageEvent
{
    private readonly UserItem _item;

    public ObjectAddMessageEvent(UserItem item)
    {
        _item = item;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.ObjectAddMessageEvent);
        RoomFloorItemWireFormat.AppendItem(packet, _item);
        return packet;
    }
}
