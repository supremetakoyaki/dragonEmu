namespace DragonEmu.Protocol.Messages.Outgoing.RoomFurniture;

public sealed class OneWayDoorStatusMessageEvent : IMessageEvent
{
    private readonly int _itemId;
    private readonly int _status;

    public OneWayDoorStatusMessageEvent(int itemId, int status)
    {
        _itemId = itemId;
        _status = status;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomFurniture.OneWayDoorStatusMessageEvent);
        packet.AppendInt32(_itemId);
        packet.AppendInt32(_status);
        return packet;
    }
}
