using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class ItemAddMessageEvent : IMessageEvent
{
    private readonly UserItem _item;

    public ItemAddMessageEvent(UserItem item)
    {
        _item = item;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.ItemAddMessageEvent);
        RoomWallItemWireFormat.AppendItem(packet, _item);
        return packet;
    }
}
