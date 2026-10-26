using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Incoming.Navigator.AuxiliaryFunctions;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class MyRoomHistorySearchMessageComposer : IMessageComposer
{
    private const int MaxListed = 50;

    private readonly IPrivateRoomRepository _privateRoomRepository;
    private readonly IRoomManager _roomManager;

    public MyRoomHistorySearchMessageComposer(IPrivateRoomRepository privateRoomRepository, IRoomManager roomManager)
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

        IReadOnlyList<PrivateRoomSummary> rooms = await _privateRoomRepository.GetVisitedByAsync(account.Id, MaxListed, cancellationToken);
        session.Send(new GuestRoomSearchResultEvent(
            GuestRoomSearchResultEvent.RoomHistory, string.Empty, RoomSearch.WithUsersNow(rooms, _roomManager)).Compose());
    }
}
