namespace DragonEmu.Protocol.Messages.Outgoing.Notifications;

public sealed class HabboBroadcastMessageEvent : IMessageEvent
{
    private readonly string _message;

    public HabboBroadcastMessageEvent(string message)
    {
        _message = message;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Notifications.HabboBroadcastMessageEvent);
        packet.AppendString(_message);
        return packet;
    }
}
