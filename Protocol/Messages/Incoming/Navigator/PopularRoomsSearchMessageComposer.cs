using System.Globalization;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class PopularRoomsSearchMessageComposer : IMessageComposer
{
    public const int MaxRooms = 50;

    private readonly IRoomManager _roomManager;

    private string _searchParam = string.Empty;

    public PopularRoomsSearchMessageComposer(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public void Parse(ClientPacket packet)
    {
        _searchParam = packet.PopString();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        int? categoryId = int.TryParse(_searchParam, NumberStyles.Integer, CultureInfo.InvariantCulture, out int category) && category >= 0
            ? category
            : null;

        List<RoomListingEntry> rooms = _roomManager.LoadedRooms
            .Where(room => room.Data.OwnerId is not null && room.UserCount > 0)
            .Where(room => categoryId is null || room.Data.CategoryId == categoryId)
            .Select(room => new RoomListingEntry(room.Data, room.UserCount))
            .OrderByDescending(entry => entry.UsersNow)
            .ThenBy(entry => entry.Room.Id)
            .Take(MaxRooms)
            .ToList();

        session.Send(new GuestRoomSearchResultEvent(GuestRoomSearchResultEvent.PopularRooms, _searchParam, rooms).Compose());
        return ValueTask.CompletedTask;
    }
}
