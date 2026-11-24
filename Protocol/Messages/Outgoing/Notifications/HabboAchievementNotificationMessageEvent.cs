namespace DragonEmu.Protocol.Messages.Outgoing.Notifications;

public sealed class HabboAchievementNotificationMessageEvent : IMessageEvent
{
    private readonly int _achievementId;
    private readonly int _level;
    private readonly string _newBadgeCode;
    private readonly string _previousBadgeCode;
    private readonly int _points;
    private readonly int _levelRewardPoints;
    private readonly int _levelRewardPointType;
    private readonly int _bonusPoints;

    public HabboAchievementNotificationMessageEvent(int achievementId, int level, string newBadgeCode, string previousBadgeCode = "",
        int points = 0, int levelRewardPoints = 0, int levelRewardPointType = Domain.ActivityPointType.Pixels, int bonusPoints = 0)
    {
        _points = points;
        _levelRewardPoints = levelRewardPoints;
        _levelRewardPointType = levelRewardPointType;
        _bonusPoints = bonusPoints;
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
        packet.AppendInt32(_points);
        packet.AppendInt32(_levelRewardPoints);
        packet.AppendInt32(_levelRewardPointType);
        packet.AppendInt32(_bonusPoints);
        packet.AppendInt32(_achievementId);
        packet.AppendString(_previousBadgeCode);
        return packet;
    }
}
