namespace DragonEmu.Protocol.Messages.Outgoing.FriendList;

public sealed class MessengerInitEvent : IMessageEvent
{
    private readonly int _userLimit;
    private readonly int _normalLimit;
    private readonly int _clubLimit;
    private readonly IReadOnlyList<FriendEntry> _friends;

    public MessengerInitEvent(int userLimit, int normalLimit, int clubLimit, IReadOnlyList<FriendEntry> friends)
    {
        _userLimit = userLimit;
        _normalLimit = normalLimit;
        _clubLimit = clubLimit;
        _friends = friends;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.FriendList.MessengerInitEvent);
        packet.AppendInt32(_userLimit);
        packet.AppendInt32(_normalLimit);
        packet.AppendInt32(_clubLimit);
        packet.AppendInt32(0);
        packet.AppendInt32(_friends.Count);
        foreach (FriendEntry friend in _friends)
        {
            FriendWireFormat.AppendFriend(packet, friend);
        }

        packet.AppendInt32(0);
        packet.AppendInt32(0);
        return packet;
    }
}
