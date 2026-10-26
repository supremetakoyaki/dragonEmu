using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.InventoryBadges;

public sealed class BadgesEvent : IMessageEvent
{
    private readonly IReadOnlyList<string> _ownedBadgeCodes;
    private readonly IReadOnlyList<(int Slot, string BadgeCode)> _wornBadges;

    public BadgesEvent(IReadOnlyList<string> ownedBadgeCodes, IReadOnlyList<(int Slot, string BadgeCode)> wornBadges)
    {
        _ownedBadgeCodes = ownedBadgeCodes;
        _wornBadges = wornBadges;
    }

    public static BadgesEvent For(IReadOnlyList<UserBadge> badges) => new(
        badges.Select(badge => badge.BadgeCode).ToList(),
        badges.Where(badge => badge.Slot is not null).OrderBy(badge => badge.Slot).Select(badge => (badge.Slot!.Value, badge.BadgeCode)).ToList());

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryBadges.BadgesEvent);

        packet.AppendInt32(_ownedBadgeCodes.Count);
        foreach (string badgeCode in _ownedBadgeCodes)
        {
            packet.AppendString(badgeCode);
        }

        packet.AppendInt32(_wornBadges.Count);
        foreach ((int slot, string badgeCode) in _wornBadges)
        {
            packet.AppendInt32(slot);
            packet.AppendString(badgeCode);
        }

        return packet;
    }
}
