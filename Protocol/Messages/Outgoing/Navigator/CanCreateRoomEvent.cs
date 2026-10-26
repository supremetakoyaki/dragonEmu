namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class CanCreateRoomEvent : IMessageEvent
{
    private readonly int _response;
    private readonly int _roomLimit;

    public CanCreateRoomEvent(int response, int roomLimit)
    {
        _response = response;
        _roomLimit = roomLimit;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.CanCreateRoomEvent);
        packet.AppendInt32(_response);
        packet.AppendInt32(_roomLimit);
        return packet;
    }
}
