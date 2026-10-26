using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class ItemUpdateMessageEvent : IMessageEvent
{
    private readonly UserItem _item;

    public ItemUpdateMessageEvent(UserItem item)
    {
        _item = item;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.ItemUpdateMessageEvent);
        RoomWallItemWireFormat.AppendItem(packet, _item);
        return packet;
    }
}
