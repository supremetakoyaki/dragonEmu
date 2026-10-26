namespace DragonEmu.Protocol.Messages.Outgoing.RoomSession;

public sealed class RoomReadyMessageEvent : IMessageEvent
{
    private readonly string _roomType;
    private readonly int _roomId;

    public RoomReadyMessageEvent(string roomType, int roomId)
    {
        _roomType = roomType;
        _roomId = roomId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomSession.RoomReadyMessageEvent);
        packet.AppendString(_roomType);
        packet.AppendInt32(_roomId);
        return packet;
    }
}
