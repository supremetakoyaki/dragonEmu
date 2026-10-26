namespace DragonEmu.Protocol.Messages.Outgoing.FriendList;

public enum MessengerErrorCode
{
    OwnListFull = 1,

    TheirListFull = 2,

    NotAcceptingRequests = 3,

    RequestNotFound = 4,
}

public sealed class MessengerErrorEvent : IMessageEvent
{
    private readonly int _clientMessageId;
    private readonly MessengerErrorCode _code;

    public MessengerErrorEvent(int clientMessageId, MessengerErrorCode code)
    {
        _clientMessageId = clientMessageId;
        _code = code;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.FriendList.MessengerErrorEvent);
        packet.AppendInt32(_clientMessageId);
        packet.AppendInt32((int)_code);
        return packet;
    }
}

public enum InstantMessageErrorCode
{
    ReceiverMuted = 3,

    SenderMuted = 4,

    Offline = 5,

    NotFriend = 6,

    Busy = 7,
}

public sealed class InstantMessageErrorEvent : IMessageEvent
{
    private readonly InstantMessageErrorCode _code;
    private readonly int _userId;

    public InstantMessageErrorEvent(InstantMessageErrorCode code, int userId)
    {
        _code = code;
        _userId = userId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.FriendList.InstantMessageErrorEvent);
        packet.AppendInt32((int)_code);
        packet.AppendInt32(_userId);
        return packet;
    }
}

public enum FollowFriendError
{
    NotFriend = 0,

    Offline = 1,

    NotInRoom = 2,

    Prevented = 3,
}

public sealed class FollowFriendFailedEvent : IMessageEvent
{
    private readonly FollowFriendError _error;

    public FollowFriendFailedEvent(FollowFriendError error)
    {
        _error = error;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.FriendList.FollowFriendFailedEvent);
        packet.AppendInt32((int)_error);
        return packet;
    }
}
