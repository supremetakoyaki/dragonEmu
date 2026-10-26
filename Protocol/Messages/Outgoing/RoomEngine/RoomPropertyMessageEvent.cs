namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class RoomPropertyMessageEvent : IMessageEvent
{
    private readonly string _type;
    private readonly string _value;

    public RoomPropertyMessageEvent(string type, string value)
    {
        _type = type;
        _value = value;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.RoomPropertyMessageEvent);
        packet.AppendString(_type);
        packet.AppendString(_value);
        return packet;
    }
}
