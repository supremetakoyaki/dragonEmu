namespace DragonEmu.Protocol.Messages.Outgoing.FriendList;

public sealed class NewConsoleMessageEvent : IMessageEvent
{
    private readonly int _senderId;
    private readonly string _message;

    public NewConsoleMessageEvent(int senderId, string message)
    {
        _senderId = senderId;
        _message = message;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.FriendList.NewConsoleMessageEvent);
        packet.AppendInt32(_senderId);
        packet.AppendString(_message);
        return packet;
    }
}
