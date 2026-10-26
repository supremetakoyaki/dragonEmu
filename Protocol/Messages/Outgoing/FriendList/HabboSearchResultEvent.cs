namespace DragonEmu.Protocol.Messages.Outgoing.FriendList;

public sealed class HabboSearchResultEvent : IMessageEvent
{
    private readonly IReadOnlyList<FriendEntry> _friends;
    private readonly IReadOnlyList<FriendEntry> _others;

    public HabboSearchResultEvent(IReadOnlyList<FriendEntry> friends, IReadOnlyList<FriendEntry> others)
    {
        _friends = friends;
        _others = others;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.FriendList.HabboSearchResultEvent);
        packet.AppendInt32(_friends.Count);
        foreach (FriendEntry hit in _friends)
        {
            FriendWireFormat.AppendSearchHit(packet, hit);
        }

        packet.AppendInt32(_others.Count);
        foreach (FriendEntry hit in _others)
        {
            FriendWireFormat.AppendSearchHit(packet, hit);
        }

        return packet;
    }
}
