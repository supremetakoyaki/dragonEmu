using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed record RoomListingEntry(PrivateRoomSummary Room, int UsersNow, RoomEvent? Event = null);

public sealed class GuestRoomSearchResultEvent : IMessageEvent
{
    public const int PopularRooms = 1;
    public const int HighestScore = 2;
    public const int FriendsRooms = 3;
    public const int RoomsWhereFriendsAre = 4;
    public const int OwnRooms = 5;
    public const int FavouriteRooms = 6;
    public const int RoomHistory = 7;
    public const int TextSearch = 8;
    public const int TagSearch = 9;
    public const int Events = 12;

    private readonly int _searchType;
    private readonly string _searchParam;
    private readonly IReadOnlyList<RoomListingEntry> _rooms;

    public GuestRoomSearchResultEvent(int searchType, string searchParam, IReadOnlyList<RoomListingEntry> rooms)
    {
        _searchType = searchType;
        _searchParam = searchParam;
        _rooms = rooms;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.GuestRoomSearchResultEvent);
        packet.AppendInt32(_searchType);
        packet.AppendString(_searchParam);
        packet.AppendInt32(_rooms.Count);

        foreach (RoomListingEntry entry in _rooms)
        {
            GuestRoomDataWireFormat.Append(packet, entry.Room, entry.UsersNow, entry.Event);
        }

        return packet;
    }
}
