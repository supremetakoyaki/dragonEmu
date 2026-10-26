using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Users;

public sealed class HabboGroupBadgesMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<GroupBadge> _badges;

    public HabboGroupBadgesMessageEvent(IReadOnlyList<GroupBadge>? badges = null)
    {
        _badges = badges ?? [];
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Users.HabboGroupBadgesMessageEvent);
        packet.AppendInt32(_badges.Count);
        foreach (GroupBadge badge in _badges)
        {
            packet.AppendInt32(badge.GroupId);
            packet.AppendString(badge.Badge);
        }

        return packet;
    }
}
