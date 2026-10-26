using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Incoming.Navigator.AuxiliaryFunctions;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class RoomTagSearchMessageComposer : IMessageComposer
{
    private const int MaxTagLength = 50;

    private readonly IPrivateRoomRepository _privateRoomRepository;
    private readonly IRoomManager _roomManager;

    private string _tag = string.Empty;

    public RoomTagSearchMessageComposer(IPrivateRoomRepository privateRoomRepository, IRoomManager roomManager)
    {
        _privateRoomRepository = privateRoomRepository;
        _roomManager = roomManager;
    }

    public void Parse(ClientPacket packet)
    {
        _tag = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is null)
        {
            return;
        }

        string tag = _tag.Trim();
        IReadOnlyList<PrivateRoomSummary> rooms = tag.Length is 0 or > MaxTagLength
            ? []
            : await _privateRoomRepository.SearchByTagAsync(
                tag, RoomSearch.OccupiedRoomIds(_roomManager), RoomSearch.MaxResults, cancellationToken);

        session.Send(new GuestRoomSearchResultEvent(
            GuestRoomSearchResultEvent.TagSearch, _tag, RoomSearch.BusiestFirst(rooms, _roomManager)).Compose());
    }
}
