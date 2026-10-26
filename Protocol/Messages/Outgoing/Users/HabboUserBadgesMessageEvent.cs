namespace DragonEmu.Protocol.Messages.Outgoing.Users;

public sealed class HabboUserBadgesMessageEvent : IMessageEvent
{
    private readonly int _userId;
    private readonly IReadOnlyList<(int Slot, string BadgeCode)> _wornBadges;

    public HabboUserBadgesMessageEvent(int userId, IReadOnlyList<(int Slot, string BadgeCode)> wornBadges)
    {
        _userId = userId;
        _wornBadges = wornBadges;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Users.HabboUserBadgesMessageEvent);
        packet.AppendInt32(_userId);
        packet.AppendInt32(_wornBadges.Count);
        foreach ((int slot, string badgeCode) in _wornBadges)
        {
            packet.AppendInt32(slot);
            packet.AppendString(badgeCode);
        }

        return packet;
    }
}
