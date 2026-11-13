using DragonEmu.Domain;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class ObjectUpdateMessageEvent : IMessageEvent
{
    private readonly UserItem? _item;
    private readonly RoomFloorItem? _floorItem;
    private readonly string? _extraData;

    public ObjectUpdateMessageEvent(UserItem item)
    {
        _item = item;
    }

    public ObjectUpdateMessageEvent(RoomFloorItem item, string extraData)
    {
        _floorItem = item;
        _extraData = extraData;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.ObjectUpdateMessageEvent);
        if (_item is not null)
        {
            RoomFloorItemWireFormat.AppendItem(packet, _item);
        }
        else
        {
            RoomFloorItemWireFormat.AppendItem(packet, _floorItem!, _extraData ?? string.Empty);
        }

        return packet;
    }
}
