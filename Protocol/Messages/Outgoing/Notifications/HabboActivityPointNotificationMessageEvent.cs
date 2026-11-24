using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Notifications;

public sealed class HabboActivityPointNotificationMessageEvent : IMessageEvent
{
    private readonly long _activityPoints;
    private readonly int _change;
    private readonly int _type;

    public HabboActivityPointNotificationMessageEvent(long activityPoints, int change = 0, int type = ActivityPointType.Pixels)
    {
        _activityPoints = activityPoints;
        _change = change;
        _type = type;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Notifications.HabboActivityPointNotificationMessageEvent);
        packet.AppendInt32((int)_activityPoints);
        packet.AppendInt32(_change);
        packet.AppendInt32(_type);
        return packet;
    }
}
