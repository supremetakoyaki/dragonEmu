namespace DragonEmu.Protocol.Messages.Outgoing.RoomChat;

public sealed class UserTypingMessageEvent : IMessageEvent
{
    private readonly int _virtualId;
    private readonly bool _isTyping;

    public UserTypingMessageEvent(int virtualId, bool isTyping)
    {
        _virtualId = virtualId;
        _isTyping = isTyping;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomChat.UserTypingMessageEvent);
        packet.AppendInt32(_virtualId);
        packet.AppendBoolean(_isTyping);
        return packet;
    }
}
