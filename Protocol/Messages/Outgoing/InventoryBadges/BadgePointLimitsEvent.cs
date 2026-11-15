namespace DragonEmu.Protocol.Messages.Outgoing.InventoryBadges;

public sealed class BadgePointLimitsEvent : IMessageEvent
{
    private readonly IReadOnlyList<(string Name, IReadOnlyList<(int Level, int Limit)> Levels)> _achievements;

    public BadgePointLimitsEvent(IReadOnlyList<(string Name, IReadOnlyList<(int Level, int Limit)> Levels)> achievements)
    {
        _achievements = achievements;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryBadges.BadgePointLimitsEvent);
        packet.AppendInt32(_achievements.Count);
        foreach ((string name, IReadOnlyList<(int Level, int Limit)> levels) in _achievements)
        {
            packet.AppendString(name);
            packet.AppendInt32(levels.Count);
            foreach ((int level, int limit) in levels)
            {
                packet.AppendInt32(level);
                packet.AppendInt32(limit);
            }
        }

        return packet;
    }
}
