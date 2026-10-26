namespace DragonEmu.Protocol.Messages.Outgoing.Notifications;

public sealed class HabboActivityPointNotificationMessageEvent : IMessageEvent
{
    private readonly long _activityPoints;
    private readonly int _change;

    public HabboActivityPointNotificationMessageEvent(long activityPoints, int change = 0)
    {
        _activityPoints = activityPoints;
        _change = change;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Notifications.HabboActivityPointNotificationMessageEvent);
        packet.AppendInt32((int)_activityPoints);
        packet.AppendInt32(_change);
        return packet;
    }
}
