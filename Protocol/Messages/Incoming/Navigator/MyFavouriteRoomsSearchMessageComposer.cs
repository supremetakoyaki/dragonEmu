using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Incoming.Navigator.AuxiliaryFunctions;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class MyFavouriteRoomsSearchMessageComposer : IMessageComposer
{
    private readonly IPrivateRoomRepository _privateRoomRepository;
    private readonly IRoomManager _roomManager;

    public MyFavouriteRoomsSearchMessageComposer(IPrivateRoomRepository privateRoomRepository, IRoomManager roomManager)
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

        IReadOnlyList<PrivateRoomSummary> rooms = await _privateRoomRepository.GetFavouritedByAsync(account.Id, cancellationToken);
        session.Send(new GuestRoomSearchResultEvent(GuestRoomSearchResultEvent.FavouriteRooms, string.Empty, RoomSearch.WithUsersNow(rooms, _roomManager)).Compose());
    }
}
