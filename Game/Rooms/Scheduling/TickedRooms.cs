using System.Collections.Concurrent;

namespace DragonEmu.Game.Rooms;

public sealed class TickedRooms
{
    private readonly ConcurrentDictionary<int, Room> _rooms = new();

    public void Mark(Room room) => _rooms[room.Id] = room;

    public bool Contains(int roomId) => _rooms.ContainsKey(roomId);

    public void Remove(Room room) => _rooms.TryRemove(new KeyValuePair<int, Room>(room.Id, room));

    public void Forget(Room room) => Remove(room);

    public List<Room> Live(IRoomManager roomManager)
    {
        var live = new List<Room>(_rooms.Count);
        foreach ((int roomId, Room room) in _rooms)
        {
            if (roomManager.TryGetLoadedRoom(roomId, out Room? loaded) && ReferenceEquals(loaded, room))
            {
                live.Add(room);
            }
            else
            {
                Remove(room);
            }
        }

        return live;
    }
}
