namespace DragonEmu.Protocol.Messages.Outgoing.Notifications;

public sealed class HabboAchievementNotificationMessageEvent : IMessageEvent
{
    private readonly int _achievementId;
    private readonly int _level;
    private readonly string _newBadgeCode;
    private readonly string _previousBadgeCode;

    public HabboAchievementNotificationMessageEvent(int achievementId, int level, string newBadgeCode, string previousBadgeCode = "")
    {
        _achievementId = achievementId;
        _level = level;
        _newBadgeCode = newBadgeCode;
        _previousBadgeCode = previousBadgeCode;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Notifications.HabboAchievementNotificationMessageEvent);
        packet.AppendInt32(_achievementId);
        packet.AppendInt32(_level);
        packet.AppendString(_newBadgeCode);
        packet.AppendString(_previousBadgeCode);
        return packet;
    }
}
