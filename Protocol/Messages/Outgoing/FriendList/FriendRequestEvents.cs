using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.FriendList;

internal static class FriendRequestWireFormat
{
    public static void Append(ServerPacket packet, FriendRequest request)
    {
        packet.AppendInt32(request.FromUserId);
        packet.AppendString(request.FromUsername);
        packet.AppendString(request.FromUserId.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}

public sealed class NewBuddyRequestEvent : IMessageEvent
{
    private readonly FriendRequest _request;

    public NewBuddyRequestEvent(FriendRequest request)
    {
        _request = request;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.FriendList.NewBuddyRequestEvent);
        FriendRequestWireFormat.Append(packet, _request);
        return packet;
    }
}

public sealed class BuddyRequestsEvent : IMessageEvent
{
    private readonly IReadOnlyList<FriendRequest> _requests;

    public BuddyRequestsEvent(IReadOnlyList<FriendRequest> requests)
    {
        _requests = requests;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.FriendList.BuddyRequestsEvent);
        packet.AppendInt32(_requests.Count);
        packet.AppendInt32(_requests.Count);
        foreach (FriendRequest request in _requests)
        {
            FriendRequestWireFormat.Append(packet, request);
        }

        return packet;
    }
}

public sealed record AcceptBuddyFailure(string RequesterName, MessengerErrorCode Reason);

public sealed class AcceptBuddyResultEvent : IMessageEvent
{
    private readonly IReadOnlyList<AcceptBuddyFailure> _failures;

    public AcceptBuddyResultEvent(IReadOnlyList<AcceptBuddyFailure> failures)
    {
        _failures = failures;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.FriendList.AcceptBuddyResultEvent);
        packet.AppendInt32(_failures.Count);
        foreach (AcceptBuddyFailure failure in _failures)
        {
            packet.AppendString(failure.RequesterName);
            packet.AppendInt32((int)failure.Reason);
        }

        return packet;
    }
}
