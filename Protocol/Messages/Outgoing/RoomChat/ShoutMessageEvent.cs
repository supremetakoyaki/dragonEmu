namespace DragonEmu.Protocol.Messages.Outgoing.RoomChat;

public sealed class ShoutMessageEvent : IMessageEvent
{
    private readonly int _virtualId;
    private readonly string _message;

    public ShoutMessageEvent(int virtualId, string message)
    {
        _virtualId = virtualId;
        _message = message;
    }

    public ServerPacket Compose() => SpeechMessage.Compose(OutgoingOpcodes.RoomChat.ShoutMessageEvent, _virtualId, _message);
}
