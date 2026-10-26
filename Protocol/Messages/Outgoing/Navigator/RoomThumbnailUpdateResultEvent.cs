namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class RoomThumbnailUpdateResultEvent : IMessageEvent
{
    public const int Saved = 1;

    private readonly int _roomId;

    public RoomThumbnailUpdateResultEvent(int roomId)
    {
        _roomId = roomId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.RoomThumbnailUpdateResultEvent);
        packet.AppendInt32(_roomId);
        packet.AppendInt32(Saved);
        return packet;
    }
}
