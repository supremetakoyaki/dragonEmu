namespace DragonEmu.Protocol.Messages.Outgoing.RoomSession;

public sealed class GamePlayerValueMessageEvent : IMessageEvent
{
    private readonly int _userId;
    private readonly int _value;

    public GamePlayerValueMessageEvent(int userId, int value)
    {
        _userId = userId;
        _value = value;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomSession.GamePlayerValueMessageEvent);
        packet.AppendInt32(_userId);
        packet.AppendInt32(_value);
        return packet;
    }
}
