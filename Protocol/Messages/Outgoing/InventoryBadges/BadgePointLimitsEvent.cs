namespace DragonEmu.Protocol.Messages.Outgoing.InventoryBadges;

public sealed class BadgePointLimitsEvent : IMessageEvent
{
    private readonly IReadOnlyList<(string BadgeCode, int Limit)> _limits;

    public BadgePointLimitsEvent(IReadOnlyList<(string BadgeCode, int Limit)> limits)
    {
        _limits = limits;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryBadges.BadgePointLimitsEvent);
        packet.AppendInt32(_limits.Count);
        foreach ((string badgeCode, int limit) in _limits)
        {
            packet.AppendString(badgeCode);
            packet.AppendInt32(limit);
        }

        return packet;
    }
}
