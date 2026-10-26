namespace DragonEmu.Protocol.Messages.Outgoing.Users;

public sealed class UserNameChangedMessageEvent : IMessageEvent
{
    private readonly int _userId;
    private readonly int _virtualId;
    private readonly string _name;

    public UserNameChangedMessageEvent(int userId, int virtualId, string name)
    {
        _userId = userId;
        _virtualId = virtualId;
        _name = name;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Users.UserNameChangedMessageEvent);
        packet.AppendInt32(_userId);
        packet.AppendInt32(_virtualId);
        packet.AppendString(_name);
        return packet;
    }
}
