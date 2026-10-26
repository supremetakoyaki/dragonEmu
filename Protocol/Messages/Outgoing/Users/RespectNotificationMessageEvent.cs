namespace DragonEmu.Protocol.Messages.Outgoing.Users;

public sealed class RespectNotificationMessageEvent : IMessageEvent
{
    private readonly int _userId;
    private readonly int _respectReceived;

    public RespectNotificationMessageEvent(int userId, int respectReceived)
    {
        _userId = userId;
        _respectReceived = respectReceived;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Users.RespectNotificationMessageEvent);
        packet.AppendInt32(_userId);
        packet.AppendInt32(_respectReceived);
        return packet;
    }
}
