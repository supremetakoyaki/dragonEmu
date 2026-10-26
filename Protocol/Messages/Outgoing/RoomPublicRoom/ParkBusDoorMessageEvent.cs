namespace DragonEmu.Protocol.Messages.Outgoing.RoomPublicRoom;

public sealed class ParkBusDoorMessageEvent : IMessageEvent
{
    private readonly bool _open;

    public ParkBusDoorMessageEvent(bool open)
    {
        _open = open;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomPublicRoom.ParkBusDoorMessageEvent);
        packet.AppendInt32(_open ? 1 : 0);
        return packet;
    }
}
