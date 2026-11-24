namespace DragonEmu.Protocol.Messages.Outgoing.Notifications;

public sealed class MOTDNotificationEvent : IMessageEvent
{
    private readonly IReadOnlyList<string> _messages;

    public MOTDNotificationEvent(IReadOnlyList<string> messages)
    {
        _messages = messages;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Notifications.MOTDNotificationEvent);
        packet.AppendInt32(_messages.Count);
        foreach (string message in _messages)
        {
            packet.AppendString(message);
        }

        return packet;
    }
}
