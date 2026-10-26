namespace DragonEmu.Game.Rooms;

public interface IRoomManager
{
    Task<Room?> GetOrLoadRoomAsync(int roomId, CancellationToken cancellationToken = default);

    int GetLiveUserCount(int roomId);

    bool TryGetLoadedRoom(int roomId, out Room? room);

    event Action<int>? RoomUnloaded;

    event Action<Room>? RoomLoaded;

    event Action<Room>? RoomRemoved;

    bool TryUnload(int roomId, out Room? room);

    int LoadedRoomCount { get; }

    IReadOnlyList<Room> LoadedRooms { get; }

    IReadOnlyList<Room> GetLoadedRoomsByHeightmapLabel(string heightmapLabel);
}
