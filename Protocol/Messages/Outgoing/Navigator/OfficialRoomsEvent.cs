using DragonEmu.Domain;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class OfficialRoomsEvent : IMessageEvent
{
    private const int EntryTypeFixedPublicRoom = 3;
    private const int EntryTypeCategory = 4;
    private const int RootParentId = 0;

    private readonly IReadOnlyList<NavigatorPublicRoom> _rooms;
    private readonly IReadOnlyList<NavigatorPublicRoomCategory> _categories;
    private readonly IRoomManager _roomManager;

    public OfficialRoomsEvent(
        IReadOnlyList<NavigatorPublicRoom> rooms,
        IReadOnlyList<NavigatorPublicRoomCategory> categories,
        IRoomManager roomManager)
    {
        _rooms = rooms;
        _categories = categories;
        _roomManager = roomManager;
    }

    public ServerPacket Compose()
    {
        HashSet<int> knownCategoryIds = _categories.Select(c => c.Id).ToHashSet();

        var entries = new List<(int Id, NavigatorPublicRoomCategory? Category, NavigatorPublicRoom? Room, int ParentId)>();
        int nextId = 1;

        var topLevel = new List<(int OrderNum, int Kind, NavigatorPublicRoomCategory? Category, NavigatorPublicRoom? Room)>();
        topLevel.AddRange(_categories.Select(c => (c.OrderNum, 0, (NavigatorPublicRoomCategory?)c, (NavigatorPublicRoom?)null)));
        topLevel.AddRange(_rooms
            .Where(r => !knownCategoryIds.Contains(r.CategoryRef))
            .Select(r => (r.OrderNum, 1, (NavigatorPublicRoomCategory?)null, (NavigatorPublicRoom?)r)));

        foreach (var item in topLevel.OrderBy(t => t.OrderNum).ThenBy(t => t.Kind))
        {
            int id = nextId++;
            entries.Add((id, item.Category, item.Room, RootParentId));

            if (item.Category is { } category)
            {
                foreach (NavigatorPublicRoom child in _rooms.Where(r => r.CategoryRef == category.Id).OrderBy(r => r.OrderNum))
                {
                    entries.Add((nextId++, null, child, id));
                }
            }
        }

        var packet = new ServerPacket(OutgoingOpcodes.Navigator.OfficialRoomsEvent);
        packet.AppendInt32(entries.Count);

        foreach ((int id, NavigatorPublicRoomCategory? category, NavigatorPublicRoom? room, int parentId) in entries)
        {
            packet.AppendInt32(id);

            if (category is not null)
            {
                packet.AppendString(category.Caption);
                packet.AppendString(string.Empty);
                packet.AppendInt32(0);
                packet.AppendString(string.Empty);
                packet.AppendString(category.Image);
                packet.AppendInt32(parentId);
                packet.AppendInt32(0);
                packet.AppendInt32(EntryTypeCategory);
                continue;
            }

            packet.AppendString(room!.Caption);
            packet.AppendString(room.Description);
            packet.AppendInt32(room.ImageSize == NavigatorImageSize.Square ? 1 : 0);
            packet.AppendString(room.Caption);
            packet.AppendString(room.ImageType == NavigatorImageType.External ? room.Image : string.Empty);
            packet.AppendInt32(parentId);
            packet.AppendInt32(_roomManager.GetLiveUserCount(room.RoomId));
            packet.AppendInt32(EntryTypeFixedPublicRoom);

            packet.AppendString(string.Empty);
            packet.AppendInt32(0);
            packet.AppendInt32(0);
            packet.AppendString(room.ImageType == NavigatorImageType.Internal ? room.Image : string.Empty);
            packet.AppendInt32(room.MaxUsers);
            packet.AppendInt32(room.RoomId);
        }

        return packet;
    }
}
