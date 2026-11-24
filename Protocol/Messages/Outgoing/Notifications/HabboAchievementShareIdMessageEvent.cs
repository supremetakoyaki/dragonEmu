namespace DragonEmu.Protocol.Messages.Outgoing.Notifications;

public sealed class HabboAchievementShareIdMessageEvent : IMessageEvent
{
    private readonly string _badgeId;
    private readonly string _shareId;

    public HabboAchievementShareIdMessageEvent(string badgeId, string shareId)
    {
        _badgeId = badgeId;
        _shareId = shareId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Notifications.HabboAchievementShareIdMessageEvent);
        packet.AppendString(_badgeId);
        packet.AppendString(_shareId);
        return packet;
    }
}
