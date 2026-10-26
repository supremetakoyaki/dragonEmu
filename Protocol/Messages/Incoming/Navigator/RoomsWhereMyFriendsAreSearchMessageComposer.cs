using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Incoming.Navigator.AuxiliaryFunctions;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class RoomsWhereMyFriendsAreSearchMessageComposer : IMessageComposer
{
    private readonly IMessengerRepository _messengerRepository;
    private readonly ClientSessionManager _sessions;
    private readonly IRoomManager _roomManager;

    public RoomsWhereMyFriendsAreSearchMessageComposer(
        IMessengerRepository messengerRepository, ClientSessionManager sessions, IRoomManager roomManager)
    {
        _messengerRepository = messengerRepository;
        _sessions = sessions;
        _roomManager = roomManager;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        var rooms = new Dictionary<int, PrivateRoomSummary>();
        foreach (MessengerProfile friend in await _messengerRepository.GetFriendsAsync(account.Id, cancellationToken))
        {
            if (friend.AllowsFollowing
                && _sessions.TryGetByUserId(friend.Id, out ClientSession? friendSession)
                && friendSession!.CurrentRoomId is { } roomId
                && _roomManager.TryGetLoadedRoom(roomId, out Room? room)
                && room!.Data.OwnerId is not null)
            {
                rooms.TryAdd(roomId, room.Data);
            }
        }

        session.Send(new GuestRoomSearchResultEvent(
            GuestRoomSearchResultEvent.RoomsWhereFriendsAre,
            string.Empty,
            RoomSearch.BusiestFirst(rooms.Values, _roomManager).Take(RoomSearch.MaxResults).ToList()).Compose());
    }
}
