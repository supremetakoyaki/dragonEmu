using System.Collections.Concurrent;
using DragonEmu.Domain;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.History;
using DragonEmu.Game.Pets;
using DragonEmu.Game.Settings;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Rooms;

public sealed class RoomManager : IRoomManager, IHostedService, IDisposable
{
    private static readonly TimeSpan EvictionSweepInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan EmptyRoomGracePeriod = TimeSpan.FromSeconds(60);

    private readonly ConcurrentDictionary<int, Lazy<Task<Room?>>> _rooms = new();
    private readonly IPrivateRoomRepository _privateRoomRepository;
    private readonly IUserItemRepository _userItemRepository;
    private readonly FurnitureDefinitionCache _furnitureDefinitions;
    private readonly IPublicRoomFurnitureRepository _publicRoomFurnitureRepository;
    private readonly RoomBotService _bots;
    private readonly IModerationRepository _moderation;
    private readonly HistoryLog _history;
    private readonly IPetRepository _pets;
    private readonly PetDefinitionCache _petDefinitions;
    private readonly GameSettingsCache _gameSettings;
    private readonly PublicRoomCache _publicRooms;
    private readonly ITraxRepository _trax;
    private readonly ILogger<RoomManager> _logger;
    private readonly PeriodicTimer _sweepTimer = new(EvictionSweepInterval);
    private CancellationTokenSource? _stoppingCts;
    private Task? _sweepLoop;

    public RoomManager(
        IPrivateRoomRepository privateRoomRepository,
        IUserItemRepository userItemRepository,
        FurnitureDefinitionCache furnitureDefinitions,
        IPublicRoomFurnitureRepository publicRoomFurnitureRepository,
        RoomBotService bots,
        IModerationRepository moderation,
        HistoryLog history,
        IPetRepository pets,
        PetDefinitionCache petDefinitions,
        GameSettingsCache gameSettings,
        PublicRoomCache publicRooms,
        ITraxRepository trax,
        ILogger<RoomManager> logger)
    {
        _trax = trax;
        _publicRooms = publicRooms;
        _pets = pets;
        _petDefinitions = petDefinitions;
        _gameSettings = gameSettings;
        _privateRoomRepository = privateRoomRepository;
        _userItemRepository = userItemRepository;
        _furnitureDefinitions = furnitureDefinitions;
        _publicRoomFurnitureRepository = publicRoomFurnitureRepository;
        _bots = bots;
        _moderation = moderation;
        _history = history;
        _logger = logger;
    }

    public async Task<Room?> GetOrLoadRoomAsync(int roomId, CancellationToken cancellationToken = default)
    {
        Lazy<Task<Room?>> lazyRoom = _rooms.GetOrAdd(
            roomId,
            id => new Lazy<Task<Room?>>(() => LoadRoomAsync(id, cancellationToken)));

        Room? room = await lazyRoom.Value;
        if (room is null)
        {
            _rooms.TryRemove(roomId, out _);
        }

        return room;
    }

    public int GetLiveUserCount(int roomId)
    {
        if (!_rooms.TryGetValue(roomId, out Lazy<Task<Room?>>? lazyRoom) || !lazyRoom.IsValueCreated)
        {
            return 0;
        }

        Task<Room?> roomTask = lazyRoom.Value;
        return roomTask.IsCompletedSuccessfully && roomTask.Result is { } room ? room.UserCount : 0;
    }

    public bool TryGetLoadedRoom(int roomId, out Room? room)
    {
        if (_rooms.TryGetValue(roomId, out Lazy<Task<Room?>>? lazyRoom) && lazyRoom.IsValueCreated && lazyRoom.Value.IsCompletedSuccessfully)
        {
            room = lazyRoom.Value.Result;
            return room is not null;
        }

        room = null;
        return false;
    }

    public int LoadedRoomCount =>
        _rooms.Values.Count(lazy => lazy.IsValueCreated && lazy.Value.IsCompletedSuccessfully && lazy.Value.Result is not null);

    public IReadOnlyList<Room> LoadedRooms =>
        _rooms.Values
            .Where(lazy => lazy.IsValueCreated && lazy.Value.IsCompletedSuccessfully && lazy.Value.Result is not null)
            .Select(lazy => lazy.Value.Result!)
            .ToList();

    public IReadOnlyList<Room> GetLoadedRoomsByHeightmapLabel(string heightmapLabel) =>
        LoadedRooms.Where(room => room.Data.HeightmapLabel == heightmapLabel).ToList();

    private async Task<Room?> LoadRoomAsync(int roomId, CancellationToken cancellationToken)
    {
        PrivateRoomSummary? data = _publicRooms.RoomDataOf(roomId) ?? await _privateRoomRepository.GetByIdAsync(roomId, cancellationToken);
        if (data is null)
        {
            return null;
        }

        var room = new Room(data)
        {
            PublicExitTiles = data.OwnerId is null && roomId != _gameSettings.Current.InfobusRoomRef ? _publicRooms.ExitTilesOf(data.HeightmapLabel) : null,
        };
        PetDefinitions petDefinitions = _petDefinitions.Current;

        IReadOnlyList<UserItem> items = await _userItemRepository.GetFloorItemsInRoomAsync(roomId, cancellationToken);

        foreach (UserItem item in items)
        {
            string? reset = item.LogicClass switch
            {
                FurnitureLogicClass.Hotspot => Room.HotspotOff,
                FurnitureLogicClass.VotingChair => Room.ChairUnlit,
                _ => null,
            };
            if (reset is not null && item.ExtraData != reset)
            {
                await _userItemRepository.UpdateExtraDataAsync(item.Id, reset, cancellationToken);
            }
        }

        if (items.Any(item => item.LogicClass is FurnitureLogicClass.Hotspot or FurnitureLogicClass.VotingChair))
        {
            items = await _userItemRepository.GetFloorItemsInRoomAsync(roomId, cancellationToken);
        }
        var floorItems = new List<RoomFloorItem>(items.Count);
        foreach (UserItem item in items)
        {
            FurnitureDefinition? def = _furnitureDefinitions.GetById(item.FurniDefId);
            if (def is not null && Room.IsPetItem(def.InteractionLogic))
            {
                room.TrackPetItem(item.Id, item.ExtraData, petDefinitions.FoodFor(def.Classname)?.FullState ?? 0);
            }

            floorItems.Add(new RoomFloorItem(
                item.Id, item.X ?? 0, item.Y ?? 0, (double)(item.Z ?? 0), item.Rotation ?? 0,
                def?.XDim ?? 1, def?.YDim ?? 1,
                def?.IsWalkable ?? false, def?.IsSittable ?? false, def?.IsBed ?? false,
                def?.IsStackable ?? false, def?.ZDim ?? 0, def?.Classname ?? "",
                def?.InteractionLogic ?? FurnitureInteractionLogic.None, def?.States, def?.LogicClass ?? FurnitureLogicClass.Basic,
                def?.VisualizationClass ?? FurnitureVisualizationClass.Static, def?.InteractionIds)
            {
                IsGateOpen = item.ExtraData == "1",
            });
        }

        room.LoadFloorItems(floorItems);

        if (room.SoundFurni is { } machine && items.FirstOrDefault(item => item.Id == machine.Id)?.ExtraData == TraxService.On)
        {
            room.StartPlayback(machine.Id, await _trax.GetPlaylistAsync(machine.Id, cancellationToken), DateTime.UtcNow);
        }
        room.LoadWallItems(await _userItemRepository.GetWallItemsInRoomAsync(roomId, cancellationToken));
        room.LoadStaticFurniture(await _publicRoomFurnitureRepository.GetForModelAsync(data.HeightmapLabel, cancellationToken));
        room.WarpTiles = _publicRooms.WarpTilesOf(data.HeightmapLabel);
        room.BusEntranceTile = data.HeightmapLabel == PublicRoomCache.ParkHeightmapLabel && data.OwnerId is null ? PublicRoomTransfers.BusEntranceTile : null;
        room.AddBots(await _bots.LoadAsync(roomId, cancellationToken));
        if (_gameSettings.Current.Pets.Enabled)
        {
            await _pets.ReturnStrayPetsAsync(roomId, data.OwnerId, cancellationToken);
            foreach (PetRecord pet in await _pets.GetInRoomAsync(roomId, cancellationToken))
            {
                (int x, int y) = room.FreeTileNear(pet.X, pet.Y) ?? (pet.X, pet.Y);
                room.AddPet(pet, petDefinitions, x, y, rotation: 2, welcomesHome: true);
            }
        }

        room.MutedUntil = await _moderation.GetRoomMuteAsync(roomId, cancellationToken);
        room.UserLeft += RecordVisit;
        RoomLoaded?.Invoke(room);
        return room;
    }

    private void RecordVisit(Room room, RoomActor user) =>
        _history.Record(new RoomVisitRecord(user.AccountId, room.Id, user.EnteredAt, DateTime.UtcNow));

    public event Action<int>? RoomUnloaded;

    public event Action<Room>? RoomLoaded;

    public event Action<Room>? RoomRemoved;

    public bool TryUnload(int roomId, out Room? room)
    {
        room = _rooms.TryRemove(roomId, out Lazy<Task<Room?>>? lazyRoom) && lazyRoom.IsValueCreated && lazyRoom.Value.IsCompletedSuccessfully
            ? lazyRoom.Value.Result
            : null;
        if (room is not null)
        {
            RoomRemoved?.Invoke(room);
            RoomUnloaded?.Invoke(roomId);
        }

        return room is not null;
    }

    private void EvictIdleRooms()
    {
        DateTime cutoff = DateTime.UtcNow - EmptyRoomGracePeriod;

        foreach ((int roomId, Lazy<Task<Room?>> lazyRoom) in _rooms)
        {
            if (!lazyRoom.IsValueCreated || !lazyRoom.Value.IsCompletedSuccessfully)
            {
                continue;
            }

            Room? room = lazyRoom.Value.Result;
            if (room is not null && room.UserCount == 0 && room.EmptySince is { } emptySince && emptySince < cutoff)
            {
                _rooms.TryRemove(roomId, out _);
                RoomRemoved?.Invoke(room);
                RoomUnloaded?.Invoke(roomId);
                _logger.LogDebug("Evicted idle room {RoomId} from memory.", roomId);
            }
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        int locked = await _privateRoomRepository.LockRoomsWithBrokenPasswordsAsync(cancellationToken);
        if (locked > 0)
        {
            _logger.LogWarning("{Count} room(s) had no valid password and were changed to doorbell lock.", locked);
        }

        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _sweepLoop = RunEvictionLoopAsync(_stoppingCts.Token);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _stoppingCts?.Cancel();

        if (_sweepLoop is not null)
        {
            try
            {
                await _sweepLoop.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    private async Task RunEvictionLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _sweepTimer.WaitForNextTickAsync(cancellationToken))
            {
                EvictIdleRooms();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose() => _sweepTimer.Dispose();
}
