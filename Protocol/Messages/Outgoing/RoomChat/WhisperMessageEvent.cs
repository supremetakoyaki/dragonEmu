namespace DragonEmu.Protocol.Messages.Outgoing.RoomChat;

public sealed class WhisperMessageEvent : IMessageEvent
{
    private readonly int _virtualId;
    private readonly string _message;

    public WhisperMessageEvent(int virtualId, string message)
    {
        _virtualId = virtualId;
        _message = message;
    }

    public ServerPacket Compose() => SpeechMessage.Compose(OutgoingOpcodes.RoomChat.WhisperMessageEvent, _virtualId, _message);
}
