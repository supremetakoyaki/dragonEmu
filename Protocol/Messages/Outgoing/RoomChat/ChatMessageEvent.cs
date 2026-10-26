namespace DragonEmu.Protocol.Messages.Outgoing.RoomChat;

public sealed class ChatMessageEvent : IMessageEvent
{
    private readonly int _virtualId;
    private readonly string _message;

    public ChatMessageEvent(int virtualId, string message)
    {
        _virtualId = virtualId;
        _message = message;
    }

    public ServerPacket Compose() => SpeechMessage.Compose(OutgoingOpcodes.RoomChat.ChatMessageEvent, _virtualId, _message);
}
