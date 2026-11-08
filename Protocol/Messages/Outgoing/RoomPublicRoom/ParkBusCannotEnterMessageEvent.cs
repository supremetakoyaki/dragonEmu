namespace DragonEmu.Protocol.Messages.Outgoing.RoomPublicRoom;

public sealed class ParkBusCannotEnterMessageEvent : IMessageEvent
{
    private readonly string _reason;

    public ParkBusCannotEnterMessageEvent(string reason)
    {
        _reason = reason;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomPublicRoom.ParkBusCannotEnterMessageEvent);
        packet.AppendString(_reason);
        return packet;
    }
}
