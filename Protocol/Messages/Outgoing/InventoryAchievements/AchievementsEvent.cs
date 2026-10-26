namespace DragonEmu.Protocol.Messages.Outgoing.InventoryAchievements;

public sealed class AchievementsEvent : IMessageEvent
{
    private readonly IReadOnlyList<(int Type, int Level, string Code)> _achievements;

    public AchievementsEvent(IReadOnlyList<(int Type, int Level, string Code)> achievements)
    {
        _achievements = achievements;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryAchievements.AchievementsEvent);

        packet.AppendInt32(_achievements.Count);
        foreach ((int type, int level, string code) in _achievements)
        {
            packet.AppendInt32(type);
            packet.AppendInt32(level);
            packet.AppendString(code);
        }

        return packet;
    }
}
