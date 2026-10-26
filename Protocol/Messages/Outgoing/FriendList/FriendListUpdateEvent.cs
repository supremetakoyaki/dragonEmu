namespace DragonEmu.Protocol.Messages.Outgoing.FriendList;

public sealed record FriendListChange
{
    private FriendListChange(int kind, int removedFriendId, FriendEntry? entry)
    {
        Kind = kind;
        RemovedFriendId = removedFriendId;
        Entry = entry;
    }

    internal int Kind { get; }

    internal int RemovedFriendId { get; }

    internal FriendEntry? Entry { get; }

    public static FriendListChange Removed(int friendId) => new(-1, friendId, null);

    public static FriendListChange Updated(FriendEntry friend) => new(0, 0, friend);

    public static FriendListChange Added(FriendEntry friend) => new(1, 0, friend);
}

public sealed class FriendListUpdateEvent : IMessageEvent
{
    private readonly IReadOnlyList<FriendListChange> _changes;

    public FriendListUpdateEvent(IReadOnlyList<FriendListChange> changes)
    {
        _changes = changes;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.FriendList.FriendListUpdateEvent);
        packet.AppendInt32(0);
        packet.AppendInt32(_changes.Count);
        foreach (FriendListChange change in _changes)
        {
            packet.AppendInt32(change.Kind);
            if (change.Entry is { } friend)
            {
                FriendWireFormat.AppendFriend(packet, friend);
            }
            else
            {
                packet.AppendInt32(change.RemovedFriendId);
            }
        }

        return packet;
    }
}
