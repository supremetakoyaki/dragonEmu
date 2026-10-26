namespace DragonEmu.Protocol.Messages.Outgoing.Availability;

public sealed class InfoHotelClosingMessageEvent : IMessageEvent
{
    private readonly int _minutes;

    public InfoHotelClosingMessageEvent(int minutes)
    {
        _minutes = minutes;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Availability.InfoHotelClosingMessageEvent);
        packet.AppendInt32(_minutes);
        return packet;
    }
}
