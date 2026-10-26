namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class FlatCreatedEvent : IMessageEvent
{
    private readonly int _roomId;
    private readonly string _roomName;

    public FlatCreatedEvent(int roomId, string roomName)
    {
        _roomId = roomId;
        _roomName = roomName;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.FlatCreatedEvent);
        packet.AppendUInt32((uint)_roomId);
        packet.AppendString(_roomName);
        return packet;
    }
}
