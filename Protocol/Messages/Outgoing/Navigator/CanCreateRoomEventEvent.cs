namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class CanCreateRoomEventEvent : IMessageEvent
{
    private readonly int _errorCode;

    public CanCreateRoomEventEvent(int errorCode)
    {
        _errorCode = errorCode;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.CanCreateRoomEventEvent);
        packet.AppendBoolean(_errorCode == 0);
        packet.AppendInt32(_errorCode);
        return packet;
    }
}
