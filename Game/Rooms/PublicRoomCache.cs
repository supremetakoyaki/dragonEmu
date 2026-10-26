using DragonEmu.Domain;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Rooms;

public sealed class PublicRoomCache : IHostedService
{
    public const string ParkHeightmapLabel = "park_a";

    private static readonly IReadOnlyDictionary<(int X, int Y), PublicRoomWarpTile> NoWarpTiles =
        new Dictionary<(int X, int Y), PublicRoomWarpTile>();

    private static readonly IReadOnlySet<(int X, int Y)> NoExitTiles = new HashSet<(int X, int Y)>();

    private readonly INavigatorPublicRoomRepository _rooms;
    private readonly INavigatorPublicRoomCategoryRepository _categories;
    private readonly IPublicRoomWarpTileRepository _warpTiles;
    private readonly IRoomHeightmapRepository _heightmaps;
    private readonly IPrivateRoomRepository _roomRows;
    private readonly IPublicRoomExitTileRepository _exitTiles;
    private readonly IPublicRoomLibraryRepository _libraries;
    private readonly ILogger<PublicRoomCache> _logger;

    private volatile Snapshot _snapshot = Snapshot.Empty;

    public PublicRoomCache(
        INavigatorPublicRoomRepository rooms, INavigatorPublicRoomCategoryRepository categories, IPublicRoomWarpTileRepository warpTiles,
        IRoomHeightmapRepository heightmaps, IPrivateRoomRepository roomRows,
        IPublicRoomExitTileRepository exitTiles, IPublicRoomLibraryRepository libraries, ILogger<PublicRoomCache> logger)
    {
        _libraries = libraries;
        _roomRows = roomRows;
        _exitTiles = exitTiles;
        _rooms = rooms;
        _categories = categories;
        _warpTiles = warpTiles;
        _heightmaps = heightmaps;
        _logger = logger;
    }

    public IReadOnlyList<NavigatorPublicRoom> Rooms => _snapshot.Rooms;

    public IReadOnlyList<NavigatorPublicRoomCategory> Categories => _snapshot.Categories;

    public int? ParkRoomId => _snapshot.ParkRoomId;

    public NavigatorPublicRoom? Find(int roomId) => _snapshot.RoomsById.GetValueOrDefault(roomId);

    public PrivateRoomSummary? RoomDataOf(int roomId) => _snapshot.RoomData.GetValueOrDefault(roomId);

    public bool IsPublic(int roomId) => _snapshot.RoomData.ContainsKey(roomId);

    public string LibrariesOf(string heightmapLabel) => _snapshot.Libraries.GetValueOrDefault(heightmapLabel)?.LibraryName ?? string.Empty;

    public string NavCodeOf(string heightmapLabel) => _snapshot.Libraries.GetValueOrDefault(heightmapLabel)?.NavCode ?? heightmapLabel;

    public IReadOnlySet<(int X, int Y)> ExitTilesOf(string heightmapLabel) =>
        _snapshot.ExitTiles.GetValueOrDefault(heightmapLabel) ?? NoExitTiles;

    public IReadOnlyDictionary<(int X, int Y), PublicRoomWarpTile> WarpTilesOf(string heightmapLabel) =>
        _snapshot.WarpTiles.GetValueOrDefault(heightmapLabel) ?? NoWarpTiles;

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<NavigatorPublicRoom> rooms = await _rooms.GetAllAsync(cancellationToken);
        IReadOnlyList<NavigatorPublicRoomCategory> categories = await _categories.GetAllAsync(cancellationToken);
        IReadOnlyList<PublicRoomWarpTile> warpTiles = await _warpTiles.GetAllAsync(cancellationToken);
        int? parkRoomId = await _heightmaps.GetAnyRoomIdByHeightmapLabelAsync(ParkHeightmapLabel, cancellationToken);
        IReadOnlyList<PublicRoomExitTile> exitTiles = await _exitTiles.GetAllAsync(cancellationToken);
        IReadOnlyList<PrivateRoomSummary> roomData = await _roomRows.GetPublicAsync(cancellationToken);
        IReadOnlyList<PublicRoomLibraries> libraries = await _libraries.GetAllAsync(cancellationToken);

        _snapshot = new Snapshot(
            rooms,
            categories,
            rooms.GroupBy(room => room.RoomId).ToDictionary(group => group.Key, group => group.First()),
            warpTiles.GroupBy(tile => tile.HeightmapLabel).ToDictionary(
                group => group.Key,
                group => (IReadOnlyDictionary<(int X, int Y), PublicRoomWarpTile>)group.ToDictionary(tile => (tile.X, tile.Y))),
            parkRoomId,
            roomData.ToDictionary(room => room.Id),
            exitTiles.GroupBy(tile => tile.HeightmapLabel).ToDictionary(
                group => group.Key, group => (IReadOnlySet<(int X, int Y)>)group.Select(tile => (tile.X, tile.Y)).ToHashSet()),
            libraries.GroupBy(library => library.HeightmapLabel).ToDictionary(group => group.Key, group => group.First()));

        _logger.LogInformation(
            "Public room cache loaded: {Rooms} navigator entries, {RoomRows} public rooms",
            rooms.Count, roomData.Count);
    }

    public Task StartAsync(CancellationToken cancellationToken) => ReloadAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private sealed record Snapshot(
        IReadOnlyList<NavigatorPublicRoom> Rooms,
        IReadOnlyList<NavigatorPublicRoomCategory> Categories,
        IReadOnlyDictionary<int, NavigatorPublicRoom> RoomsById,
        IReadOnlyDictionary<string, IReadOnlyDictionary<(int X, int Y), PublicRoomWarpTile>> WarpTiles,
        int? ParkRoomId,
        IReadOnlyDictionary<int, PrivateRoomSummary> RoomData,
        IReadOnlyDictionary<string, IReadOnlySet<(int X, int Y)>> ExitTiles,
        IReadOnlyDictionary<string, PublicRoomLibraries> Libraries)
    {
        public static Snapshot Empty { get; } = new(
            [], [], new Dictionary<int, NavigatorPublicRoom>(), new Dictionary<string, IReadOnlyDictionary<(int X, int Y), PublicRoomWarpTile>>(), null,
            new Dictionary<int, PrivateRoomSummary>(), new Dictionary<string, IReadOnlySet<(int X, int Y)>>(),
            new Dictionary<string, PublicRoomLibraries>());
    }
}
