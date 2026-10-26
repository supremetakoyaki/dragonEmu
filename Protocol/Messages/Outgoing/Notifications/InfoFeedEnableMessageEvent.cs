namespace DragonEmu.Protocol.Messages.Outgoing.Notifications;

public sealed class InfoFeedEnableMessageEvent : IMessageEvent
{
    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Notifications.InfoFeedEnableMessageEvent);
        packet.AppendBoolean(true);
        return packet;
    }
}
