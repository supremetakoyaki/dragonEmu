using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Incoming.Navigator.AuxiliaryFunctions;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class MyFriendsRoomsSearchMessageComposer : IMessageComposer
{
    private readonly IPrivateRoomRepository _privateRoomRepository;
    private readonly IRoomManager _roomManager;

    public MyFriendsRoomsSearchMessageComposer(IPrivateRoomRepository privateRoomRepository, IRoomManager roomManager)
    {
        _privateRoomRepository = privateRoomRepository;
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

        IReadOnlyList<PrivateRoomSummary> rooms = await _privateRoomRepository.GetOwnedByFriendsOfAsync(
            account.Id, RoomSearch.OccupiedRoomIds(_roomManager), RoomSearch.MaxResults, cancellationToken);
        session.Send(new GuestRoomSearchResultEvent(
            GuestRoomSearchResultEvent.FriendsRooms, string.Empty, RoomSearch.BusiestFirst(rooms, _roomManager)).Compose());
    }
}
