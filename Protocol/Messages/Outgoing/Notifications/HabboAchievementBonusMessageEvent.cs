namespace DragonEmu.Protocol.Messages.Outgoing.Notifications;

public sealed class HabboAchievementBonusMessageEvent : IMessageEvent
{
    private readonly int _bonusPoints;
    private readonly string _realName;

    public HabboAchievementBonusMessageEvent(int bonusPoints, string realName)
    {
        _bonusPoints = bonusPoints;
        _realName = realName;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Notifications.HabboAchievementBonusMessageEvent);
        packet.AppendInt32(_bonusPoints);
        packet.AppendString(_realName);
        return packet;
    }
}
