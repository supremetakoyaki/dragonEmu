namespace DragonEmu.Protocol.Messages.Outgoing.RoomSettings;

public sealed class FlatControllerAddedEvent : IMessageEvent
{
    private readonly int _roomId;
    private readonly int _userId;
    private readonly string _username;

    public FlatControllerAddedEvent(int roomId, int userId, string username)
    {
        _roomId = roomId;
        _userId = userId;
        _username = username;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomSettings.FlatControllerAddedEvent);
        packet.AppendInt32(_roomId);
        packet.AppendInt32(_userId);
        packet.AppendString(_username);
        return packet;
    }
}
