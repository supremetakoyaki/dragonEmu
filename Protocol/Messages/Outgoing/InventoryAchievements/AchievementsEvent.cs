namespace DragonEmu.Protocol.Messages.Outgoing.InventoryAchievements;

public sealed class AchievementsEvent : IMessageEvent
{
    private readonly IReadOnlyList<(int Type, int Level, string Code, int ScoreLimit, int LevelRewardPoints, int CurrentPoints)> _achievements;

    public AchievementsEvent(IReadOnlyList<(int Type, int Level, string Code, int ScoreLimit, int LevelRewardPoints, int CurrentPoints)> achievements)
    {
        _achievements = achievements;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryAchievements.AchievementsEvent);

        packet.AppendInt32(_achievements.Count);
        foreach ((int type, int level, string code, int scoreLimit, int levelRewardPoints, int currentPoints) in _achievements)
        {
            packet.AppendInt32(type);
            packet.AppendInt32(level);
            packet.AppendString(code);
            packet.AppendInt32(scoreLimit);
            packet.AppendInt32(levelRewardPoints);
            packet.AppendInt32(currentPoints);
        }

        return packet;
    }
}
