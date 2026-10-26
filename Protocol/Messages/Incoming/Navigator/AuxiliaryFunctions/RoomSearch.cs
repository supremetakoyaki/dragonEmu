using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator.AuxiliaryFunctions;

internal static class RoomSearch
{
    public const int MaxResults = 50;

    public static IReadOnlyCollection<int> OccupiedRoomIds(IRoomManager roomManager) =>
        roomManager.LoadedRooms.Where(room => room.UserCount > 0).Select(room => room.Data.Id).ToList();

    public static List<RoomListingEntry> WithUsersNow(IEnumerable<PrivateRoomSummary> rooms, IRoomManager roomManager) =>
        rooms.Select(room => new RoomListingEntry(room, roomManager.GetLiveUserCount(room.Id))).ToList();

    public static List<RoomListingEntry> BusiestFirst(IEnumerable<PrivateRoomSummary> rooms, IRoomManager roomManager) =>
        WithUsersNow(rooms, roomManager).OrderByDescending(entry => entry.UsersNow).ToList();
}
