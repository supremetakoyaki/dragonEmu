namespace DragonEmu.Protocol.Messages.Outgoing.RoomSettings;

public sealed class NoSuchFlatEvent : IMessageEvent
{
    private readonly int _roomId;

    public NoSuchFlatEvent(int roomId)
    {
        _roomId = roomId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomSettings.NoSuchFlatEvent);
        packet.AppendInt32(_roomId);
        return packet;
    }
}
