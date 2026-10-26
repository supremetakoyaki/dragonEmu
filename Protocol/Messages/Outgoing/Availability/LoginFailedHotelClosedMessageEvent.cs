namespace DragonEmu.Protocol.Messages.Outgoing.Availability;

public sealed class LoginFailedHotelClosedMessageEvent : IMessageEvent
{
    private readonly int _hour;
    private readonly int _minute;

    public LoginFailedHotelClosedMessageEvent(int hour, int minute)
    {
        _hour = hour;
        _minute = minute;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Availability.LoginFailedHotelClosedMessageEvent);
        packet.AppendInt32(_hour);
        packet.AppendInt32(_minute);
        return packet;
    }
}
