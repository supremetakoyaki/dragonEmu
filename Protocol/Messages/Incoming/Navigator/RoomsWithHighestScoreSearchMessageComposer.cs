using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Incoming.Navigator.AuxiliaryFunctions;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class RoomsWithHighestScoreSearchMessageComposer : IMessageComposer
{
    private readonly IPrivateRoomRepository _privateRoomRepository;
    private readonly IRoomManager _roomManager;

    public RoomsWithHighestScoreSearchMessageComposer(IPrivateRoomRepository privateRoomRepository, IRoomManager roomManager)
    {
        _privateRoomRepository = privateRoomRepository;
        _roomManager = roomManager;
    }

    public int AdIndex { get; private set; }

    public void Parse(ClientPacket packet)
    {
        AdIndex = packet.RemainingLength > 0 ? packet.PopInt32() : 0;
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is null)
        {
            return;
        }

        IReadOnlyList<PrivateRoomSummary> rooms = await _privateRoomRepository.GetHighestScoredAsync(RoomSearch.MaxResults, cancellationToken);

        session.Send(new GuestRoomSearchResultEvent(
            GuestRoomSearchResultEvent.HighestScore, string.Empty, RoomSearch.WithUsersNow(rooms, _roomManager)).Compose());
    }
}
