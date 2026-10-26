using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Incoming.Navigator.AuxiliaryFunctions;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class RoomTextSearchMessageComposer : IMessageComposer
{
    private const int MaxTextLength = 100;

    private readonly IPrivateRoomRepository _privateRoomRepository;
    private readonly IRoomManager _roomManager;

    private string _searchText = string.Empty;

    public RoomTextSearchMessageComposer(IPrivateRoomRepository privateRoomRepository, IRoomManager roomManager)
    {
        _privateRoomRepository = privateRoomRepository;
        _roomManager = roomManager;
    }

    public void Parse(ClientPacket packet)
    {
        _searchText = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is null)
        {
            return;
        }

        string text = _searchText.Trim();
        IReadOnlyList<PrivateRoomSummary> rooms = text.Length is 0 or > MaxTextLength
            ? []
            : await _privateRoomRepository.SearchByTextAsync(
                text, RoomSearch.OccupiedRoomIds(_roomManager), RoomSearch.MaxResults, cancellationToken);

        session.Send(new GuestRoomSearchResultEvent(
            GuestRoomSearchResultEvent.TextSearch, _searchText, RoomSearch.BusiestFirst(rooms, _roomManager)).Compose());
    }
}
