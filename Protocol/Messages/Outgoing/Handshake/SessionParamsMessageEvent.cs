namespace DragonEmu.Protocol.Messages.Outgoing.Handshake;

public sealed class SessionParamsMessageEvent : IMessageEvent
{
    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Handshake.SessionParamsMessageEvent);
        packet.AppendInt32(9);

        packet.AppendInt32(0);
        packet.AppendInt32(0);
        packet.AppendInt32(1);
        packet.AppendInt32(1);
        packet.AppendInt32(3);
        packet.AppendInt32(0);
        packet.AppendInt32(2);
        packet.AppendInt32(1);
        packet.AppendInt32(4);
        packet.AppendInt32(0);
        packet.AppendInt32(5);
        packet.AppendString("dd-MM-yyyy");
        packet.AppendInt32(7);
        packet.AppendInt32(0);
        packet.AppendInt32(8);
        packet.AppendString("hotel-co.uk");
        packet.AppendInt32(9);
        packet.AppendInt32(0);

        return packet;
    }
}
