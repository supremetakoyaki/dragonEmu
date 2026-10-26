namespace DragonEmu.Protocol.Messages.Outgoing.RoomSettings;

public sealed class FlatControllerRemovedEvent : IMessageEvent
{
    private readonly int _roomId;
    private readonly int _userId;

    public FlatControllerRemovedEvent(int roomId, int userId)
    {
        _roomId = roomId;
        _userId = userId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomSettings.FlatControllerRemovedEvent);
        packet.AppendInt32(_roomId);
        packet.AppendInt32(_userId);
        return packet;
    }
}
