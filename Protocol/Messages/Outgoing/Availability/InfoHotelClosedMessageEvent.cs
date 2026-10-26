namespace DragonEmu.Protocol.Messages.Outgoing.Availability;

public sealed class InfoHotelClosedMessageEvent : IMessageEvent
{
    private readonly int _hour;
    private readonly int _minute;
    private readonly bool _userThrownOut;

    public InfoHotelClosedMessageEvent(int hour, int minute, bool userThrownOut)
    {
        _hour = hour;
        _minute = minute;
        _userThrownOut = userThrownOut;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Availability.InfoHotelClosedMessageEvent);
        packet.AppendInt32(_hour);
        packet.AppendInt32(_minute);
        packet.AppendInt32(_userThrownOut ? 1 : 0);
        return packet;
    }
}
