using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Users;

public sealed class HabboGroupDetailsMessageEvent : IMessageEvent
{
    private readonly GroupDetails? _group;

    public HabboGroupDetailsMessageEvent(GroupDetails? group)
    {
        _group = group;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Users.HabboGroupDetailsMessageEvent);
        if (_group is null)
        {
            packet.AppendInt32(-1);
            return packet;
        }

        packet.AppendInt32(_group.Id);
        packet.AppendString(_group.Name);
        packet.AppendString(_group.Description);
        packet.AppendInt32(_group.RoomId ?? 0);
        packet.AppendString(_group.RoomName);
        return packet;
    }
}
