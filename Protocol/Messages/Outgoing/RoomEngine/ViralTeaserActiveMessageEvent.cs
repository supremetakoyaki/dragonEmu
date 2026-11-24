namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class ViralTeaserActiveMessageEvent : IMessageEvent
{
    private readonly int _type;

    public ViralTeaserActiveMessageEvent(int type)
    {
        _type = type;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.ViralTeaserActiveMessageEvent);
        packet.AppendInt32(_type);
        return packet;
    }
}
