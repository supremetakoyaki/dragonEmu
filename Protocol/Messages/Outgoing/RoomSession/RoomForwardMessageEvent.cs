namespace DragonEmu.Protocol.Messages.Outgoing.RoomSession;

public sealed class RoomForwardMessageEvent : IMessageEvent
{
    private readonly bool _isPublic;
    private readonly int _roomId;

    public RoomForwardMessageEvent(bool isPublic, int roomId)
    {
        _isPublic = isPublic;
        _roomId = roomId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomSession.RoomForwardMessageEvent);
        packet.AppendBoolean(_isPublic);
        packet.AppendInt32(_roomId);
        return packet;
    }
}
