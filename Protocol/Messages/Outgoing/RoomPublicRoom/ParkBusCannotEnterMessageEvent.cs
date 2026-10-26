namespace DragonEmu.Protocol.Messages.Outgoing.RoomPublicRoom;

public sealed class ParkBusCannotEnterMessageEvent : IMessageEvent
{
    public const string DefaultReason = "The Infobus is not currently running.";

    private readonly string _reason;

    public ParkBusCannotEnterMessageEvent(string reason = DefaultReason)
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
