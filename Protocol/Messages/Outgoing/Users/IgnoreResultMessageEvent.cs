namespace DragonEmu.Protocol.Messages.Outgoing.Users;

public sealed class IgnoreResultMessageEvent : IMessageEvent
{
    private readonly int _result;

    public IgnoreResultMessageEvent(int result)
    {
        _result = result;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Users.IgnoreResultMessageEvent);
        packet.AppendInt32(_result);
        return packet;
    }
}
