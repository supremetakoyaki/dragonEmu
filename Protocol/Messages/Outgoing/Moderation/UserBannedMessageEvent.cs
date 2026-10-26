namespace DragonEmu.Protocol.Messages.Outgoing.Moderation;

public sealed class UserBannedMessageEvent : IMessageEvent
{
    private readonly string _message;

    public UserBannedMessageEvent(string message)
    {
        _message = message;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Moderation.UserBannedMessageEvent);
        packet.AppendString(_message);
        return packet;
    }
}
