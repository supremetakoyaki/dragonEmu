namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class RoomInfoUpdatedEvent : IMessageEvent
{
    private readonly int _roomId;

    public RoomInfoUpdatedEvent(int roomId)
    {
        _roomId = roomId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.RoomInfoUpdatedEvent);
        packet.AppendInt32(_roomId);
        return packet;
    }
}
