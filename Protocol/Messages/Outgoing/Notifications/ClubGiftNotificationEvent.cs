namespace DragonEmu.Protocol.Messages.Outgoing.Notifications;

public sealed class ClubGiftNotificationEvent : IMessageEvent
{
    private readonly int _giftsAvailableNow;

    public ClubGiftNotificationEvent(int giftsAvailableNow)
    {
        _giftsAvailableNow = giftsAvailableNow;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Notifications.ClubGiftNotificationEvent);
        packet.AppendInt32(_giftsAvailableNow);
        return packet;
    }
}
