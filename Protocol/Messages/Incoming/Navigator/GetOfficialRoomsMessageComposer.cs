using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class GetOfficialRoomsMessageComposer : IMessageComposer
{
    private readonly PublicRoomCache _publicRooms;
    private readonly IRoomManager _roomManager;

    public GetOfficialRoomsMessageComposer(PublicRoomCache publicRooms, IRoomManager roomManager)
    {
        _publicRooms = publicRooms;
        _roomManager = roomManager;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not null)
        {
            IReadOnlyList<NavigatorPublicRoom> rooms = _publicRooms.Rooms.Where(r => r.Visible).ToList();
            session.Send(new OfficialRoomsEvent(rooms, _publicRooms.Categories, _roomManager).Compose());
        }

        return ValueTask.CompletedTask;
    }
}
