using System.Collections.Concurrent;
using DragonEmu.Domain;
using DragonEmu.Game.Rooms.Pathfinding;

namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    public const double MaxClimbHeight = 1.0;

    private readonly ConcurrentDictionary<long, RoomActor> _actors = new();

    private int _userCount;
    private int _nextVirtualId;
    private readonly Lazy<RoomGrid> _grid;

    private readonly ConcurrentQueue<RoomActor> _walkedOut = new();

    private readonly object _furnitureLock = new();
    private readonly Dictionary<int, RoomFloorItem> _floorItems = new();
    private Dictionary<(int X, int Y), RoomFloorItem> _topItemByTile = new();

    private Dictionary<(int X, int Y), List<RoomFloorItem>> _itemsByTile = new();

    private readonly Dictionary<int, UserItem> _wallItems = new();

    private readonly object _teleportLock = new();
    private readonly Dictionary<int, TeleporterSequence> _teleporterSequences = new();

    private readonly object _movementLock = new();

    private readonly object _rollLock = new();
    private readonly Dictionary<int, FurnitureRollSequence> _rolls = new();

    public Room(PrivateRoomSummary data)
    {
        Data = data;
        _grid = new Lazy<RoomGrid>(() => new RoomGrid(data.Heightmap, (data.DoorX, data.DoorY, data.DoorZ), data.MaxStepHeight, doorIsFloor: PublicExitTiles is not null));
        Wallpaper = data.Wallpaper;
        Floor = data.Floor;
        Landscape = data.Landscape;
    }

    public PrivateRoomSummary Data { get; private set; }

    public void ApplySettingsUpdate(PrivateRoomSummary updatedData) => Data = updatedData;

    public void SetQueuetileSpeed(int speed) => Data = Data with { QueuetileSpeed = speed };

    public string Wallpaper { get; set; }

    public string Floor { get; set; }

    public string Landscape { get; set; }

    public int Id => Data.Id;

    public DateTime? MutedUntil { get; set; }

    public int UserCount => Volatile.Read(ref _userCount);

    public IReadOnlyCollection<RoomActor> Actors => _actors.Values.ToArray();

    public RoomGrid Grid => _grid.Value;

    public DateTime? EmptySince { get; private set; } = DateTime.UtcNow;

    public int NextVirtualId() => Interlocked.Increment(ref _nextVirtualId);

    public event Action<Room, RoomActor>? UserLeft;

    private volatile bool _petActivityPaused;

    public bool PetActivityPaused
    {
        get => _petActivityPaused;
        set => _petActivityPaused = value;
    }

    public event Action<Room, RoomActor>? UserEntered;

    public bool TryAddActor(RoomActor actor)
    {
        bool added = _actors.TryAdd(actor.SessionId, actor);
        if (added && actor.Type == ActorType.Player)
        {
            Interlocked.Increment(ref _userCount);
            EmptySince = null;
            UserEntered?.Invoke(this, actor);
        }

        return added;
    }

    public bool TryRemoveActor(long sessionId, out RoomActor? actor)
    {
        bool removed = _actors.TryRemove(sessionId, out actor);
        if (!removed)
        {
            return false;
        }

        actor!.TakeTrade()?.Cancel(actor.AccountId);
        EndEventHostedBy(sessionId);

        if (actor.Type == ActorType.Player)
        {
            if (Interlocked.Decrement(ref _userCount) == 0)
            {
                EmptySince = DateTime.UtcNow;
            }

            UserLeft?.Invoke(this, actor);
        }

        return true;
    }

    public bool TryGetActor(long sessionId, out RoomActor? actor) => _actors.TryGetValue(sessionId, out actor);

    public bool TryGetActorByVirtualId(int virtualId, out RoomActor? actor)
    {
        actor = _actors.Values.FirstOrDefault(candidate => candidate.VirtualId == virtualId);
        return actor is not null;
    }

    private (HashSet<(int X, int Y)> Blocked, HashSet<(int X, int Y)>? BlockedAsGoal) ObstaclesFor(RoomActor actor)
    {
        HashSet<(int X, int Y)> others = OccupiedTiles(actor.SessionId);
        HashSet<(int X, int Y)> blocked = Data.AllowWalkthrough ? [] : others;
        HashSet<(int X, int Y)>? blockedAsGoal = Data.AllowWalkthrough ? others : null;

        if (!actor.HasSwimwear)
        {
            blocked.UnionWith(PoolWater);
        }

        return (blocked, blockedAsGoal);
    }

    public HashSet<(int X, int Y)> OccupiedTiles(long excludeSessionId)
    {
        var tiles = new HashSet<(int X, int Y)>();
        foreach (RoomActor actor in _actors.Values)
        {
            if (actor.SessionId == excludeSessionId)
            {
                continue;
            }

            lock (actor.Lock)
            {
                tiles.Add((actor.X, actor.Y));
            }
        }

        return tiles;
    }

    public HashSet<(int X, int Y)> OccupiedTiles(IReadOnlySet<long> excludeSessionIds)
    {
        var tiles = new HashSet<(int X, int Y)>();
        foreach (RoomActor actor in _actors.Values)
        {
            if (excludeSessionIds.Contains(actor.SessionId))
            {
                continue;
            }

            lock (actor.Lock)
            {
                tiles.Add((actor.X, actor.Y));
            }
        }

        return tiles;
    }

    public RoomFloorItem? GetFloorItem(int itemId)
    {
        lock (_furnitureLock)
        {
            return _floorItems.GetValueOrDefault(itemId);
        }
    }

    public int FloorItemCount
    {
        get
        {
            lock (_furnitureLock)
            {
                return _floorItems.Keys.Count(id => id > 0);
            }
        }
    }

    public IReadOnlyList<RoomFloorItem> FloorItemsWhere(Func<RoomFloorItem, bool> predicate)
    {
        lock (_furnitureLock)
        {
            return _floorItems.Values.Where(predicate).ToArray();
        }
    }

    public int CountFloorItemsWithInteractionLogic(FurnitureInteractionLogic interactionLogic)
    {
        lock (_furnitureLock)
        {
            return _floorItems.Values.Count(i => i.Id > 0 && i.InteractionLogic == interactionLogic);
        }
    }

    public UserItem? GetWallItemWithLogicClass(FurnitureLogicClass logicClass)
    {
        lock (_furnitureLock)
        {
            return _wallItems.Values.FirstOrDefault(item => item.LogicClass == logicClass);
        }
    }

    public int CountWallItemsWithLogicClass(FurnitureLogicClass logicClass)
    {
        lock (_furnitureLock)
        {
            return _wallItems.Values.Count(i => i.LogicClass == logicClass);
        }
    }

    public UserItem? GetWallItem(int itemId)
    {
        lock (_furnitureLock)
        {
            return _wallItems.GetValueOrDefault(itemId);
        }
    }

    public int WallItemCount
    {
        get
        {
            lock (_furnitureLock)
            {
                return _wallItems.Count;
            }
        }
    }

    public void LoadWallItems(IEnumerable<UserItem> items)
    {
        lock (_furnitureLock)
        {
            _wallItems.Clear();
            foreach (UserItem item in items)
            {
                _wallItems[item.Id] = item;
            }
        }
    }

    public void AddWallItem(UserItem item)
    {
        lock (_furnitureLock)
        {
            _wallItems[item.Id] = item;
        }
    }

    public void RemoveWallItem(int itemId)
    {
        lock (_furnitureLock)
        {
            _wallItems.Remove(itemId);
        }
    }

    public UserItem? SetWallItemExtraData(int itemId, string? extraData)
    {
        lock (_furnitureLock)
        {
            if (!_wallItems.TryGetValue(itemId, out UserItem? item))
            {
                return null;
            }

            UserItem updated = item with { ExtraData = extraData };
            _wallItems[itemId] = updated;
            return updated;
        }
    }

    public UserItem? SetWallItemPosition(int itemId, string wallPosition)
    {
        lock (_furnitureLock)
        {
            if (!_wallItems.TryGetValue(itemId, out UserItem? item))
            {
                return null;
            }

            UserItem updated = item with { WallPosition = wallPosition };
            _wallItems[itemId] = updated;
            return updated;
        }
    }

    public IReadOnlyList<long> RestingActorsOn(RoomFloorItem item)
    {
        HashSet<(int X, int Y)> footprint = item.OccupiedTiles().ToHashSet();
        var resting = new List<long>();
        foreach (RoomActor actor in _actors.Values)
        {
            lock (actor.Lock)
            {
                if (actor.Posture != ActorPosture.Stand && footprint.Contains((actor.X, actor.Y)))
                {
                    resting.Add(actor.SessionId);
                }
            }
        }

        return resting;
    }

    public IReadOnlyList<long> StandingActorsOn(IEnumerable<(int X, int Y)> footprint)
    {
        var tiles = footprint as HashSet<(int X, int Y)> ?? footprint.ToHashSet();
        var standing = new List<long>();
        foreach (RoomActor actor in _actors.Values)
        {
            lock (actor.Lock)
            {
                if (actor.Posture == ActorPosture.Stand && tiles.Contains((actor.X, actor.Y)))
                {
                    standing.Add(actor.SessionId);
                }
            }
        }

        return standing;
    }

    public void LoadFloorItems(IEnumerable<RoomFloorItem> items)
    {
        lock (_furnitureLock)
        {
            _floorItems.Clear();
            foreach (RoomFloorItem item in items)
            {
                _floorItems[item.Id] = item;
            }

            RebuildTileIndex();
        }
    }

    public IReadOnlyList<PublicRoomFurnitureItem> StaticFurniture { get; private set; } = [];

    public const string QueueTileClassname = "queue_tile2";

    public const string PoolEnterClassname = "poolEnter";
    public const string PoolExitClassname = "poolExit";

    public IReadOnlyList<PublicRoomFurnitureItem> VisibleStaticFurniture { get; private set; } = [];

    public void LoadStaticFurniture(IReadOnlyList<PublicRoomFurnitureItem> furniture)
    {
        StaticFurniture = furniture;
        VisibleStaticFurniture = furniture.Where(item => !item.IsInvisible).ToList();

        foreach (PublicRoomFurnitureItem ladder in furniture.Where(item => item.Classname is PoolEnterClassname or PoolExitClassname))
        {
            Grid.AddLadder(ladder.X, ladder.Y);
        }

        LoadPool(furniture);

        lock (_furnitureLock)
        {
            foreach (PublicRoomFurnitureItem item in furniture)
            {
                double tileHeight = Grid.IsWalkable(item.X, item.Y) ? Grid.HeightAt(item.X, item.Y) : 0;
                bool seat = item.IsSittable || item.IsBed;
                var floorItem = new RoomFloorItem(
                    id: -item.Id, x: item.X, y: item.Y, z: tileHeight, rotation: (item.Rotation % 8) * 45,
                    width: item.XDim, length: item.YDim,
                    isWalkable: item.IsWalkable, isSittable: item.IsSittable, isBed: item.IsBed, isStackable: false,
                    stackHeight: item.IsWalkable && !seat ? 0 : item.ZDim,
                    classname: item.Classname,
                    interactionLogic: item.Classname == QueueTileClassname ? FurnitureInteractionLogic.Roller : FurnitureInteractionLogic.None);
                _floorItems[floorItem.Id] = floorItem;
            }

            RebuildTileIndex();
        }
    }

    public void AddFloorItem(RoomFloorItem item)
    {
        lock (_furnitureLock)
        {
            _floorItems[item.Id] = item;
            MarkPositionUnsavedIfRolled(item.Id);
            RebuildTileIndex();
        }
    }

    public void MoveFloorItem(int itemId, int x, int y, double z, int rotation)
    {
        lock (_furnitureLock)
        {
            if (_floorItems.TryGetValue(itemId, out RoomFloorItem? item))
            {
                item.MoveTo(x, y, z, rotation);
                MarkPositionUnsavedIfRolled(itemId);
                RebuildTileIndex();
            }
        }
    }

    public double? TryAddFloorItemOnStack(RoomFloorItem item, IReadOnlySet<long>? exempt = null)
    {
        lock (_movementLock)
        {
            if (IsFootprintTaken(item.OccupiedTiles(), exempt))
            {
                return null;
            }

            lock (_furnitureLock)
            {
                if (_floorItems.ContainsKey(item.Id) || StackTopUnlocked(item.X, item.Y, excludeItemId: item.Id) is not { } z)
                {
                    return null;
                }

                item.MoveTo(item.X, item.Y, z, item.Rotation);
                item.IsPositionPending = true;
                _floorItems[item.Id] = item;
                MarkPositionUnsavedIfRolled(item.Id);
                RebuildTileIndex();
                return z;
            }
        }
    }

    public double? TryMoveFloorItemOnStack(int itemId, int x, int y, int rotation, IReadOnlySet<long>? exempt = null)
    {
        lock (_movementLock)
        {
            if (GetFloorItem(itemId) is not { } moving || IsFootprintTaken(moving.OccupiedTilesAt(x, y, rotation), exempt))
            {
                return null;
            }

            lock (_furnitureLock)
            {
                if (!_floorItems.TryGetValue(itemId, out RoomFloorItem? item) || StackTopUnlocked(x, y, excludeItemId: itemId) is not { } z)
                {
                    return null;
                }

                item.MoveTo(x, y, z, rotation);
                item.IsPositionPending = true;
                MarkPositionUnsavedIfRolled(itemId);
                RebuildTileIndex();
                return z;
            }
        }
    }

    public const double RotateOntoWalkableTolerance = 0.1;

    public double? TryRotateFloorItemInPlace(int itemId, int rotation)
    {
        lock (_movementLock)
        {
            lock (_furnitureLock)
            {
                if (!_floorItems.TryGetValue(itemId, out RoomFloorItem? item))
                {
                    return null;
                }

                List<(int X, int Y)> newTiles = item.OccupiedTilesAt(item.X, item.Y, rotation).ToList();
                if (newTiles.Any(tile => !Grid.CanHoldFurniture(tile.X, tile.Y)))
                {
                    return null;
                }

                double z = item.Z;
                if (!item.IsWalkable)
                {
                    HashSet<(int X, int Y)> addedTiles = newTiles.Except(item.OccupiedTiles()).ToHashSet();
                    if (IsAnyoneOn(addedTiles))
                    {
                        return null;
                    }

                    foreach ((int x, int y) in addedTiles)
                    {
                        foreach (RoomFloorItem other in ItemsAtUnlocked(x, y))
                        {
                            if (other.Id == itemId || other.SurfaceZ <= item.Z + 0.001)
                            {
                                continue;
                            }

                            if (!other.IsWalkable || Math.Abs(other.Z - item.Z) > RotateOntoWalkableTolerance)
                            {
                                return null;
                            }

                            z = Math.Max(z, other.SurfaceZ);
                        }
                    }
                }

                z = Math.Round(z, 4);
                item.MoveTo(item.X, item.Y, z, rotation);
                item.IsPositionPending = true;
                MarkPositionUnsavedIfRolled(itemId);
                RebuildTileIndex();
                return z;
            }
        }
    }

    private bool IsAnyoneOn(IReadOnlySet<(int X, int Y)> tiles)
    {
        foreach (RoomActor actor in _actors.Values)
        {
            lock (actor.Lock)
            {
                if (tiles.Contains((actor.X, actor.Y)) || (actor.PendingStep is { } step && tiles.Contains((step.X, step.Y))))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public IReadOnlyList<long> ActorsOn(IReadOnlySet<(int X, int Y)> tiles)
    {
        var on = new List<long>();
        foreach (RoomActor actor in _actors.Values)
        {
            lock (actor.Lock)
            {
                if (tiles.Contains((actor.X, actor.Y)))
                {
                    on.Add(actor.SessionId);
                }
            }
        }

        return on;
    }

    private bool IsFootprintTaken(IEnumerable<(int X, int Y)> footprint, IReadOnlySet<long>? exempt)
    {
        HashSet<(int X, int Y)> tiles = footprint.ToHashSet();
        foreach (RoomActor actor in _actors.Values)
        {
            if (exempt?.Contains(actor.SessionId) == true)
            {
                continue;
            }

            lock (actor.Lock)
            {
                if ((actor.Posture == ActorPosture.Stand && tiles.Contains((actor.X, actor.Y)))
                    || (actor.PendingStep is { } step && tiles.Contains((step.X, step.Y))))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void ConfirmFloorItem(int itemId)
    {
        lock (_furnitureLock)
        {
            if (_floorItems.TryGetValue(itemId, out RoomFloorItem? item))
            {
                item.IsPositionPending = false;
            }
        }
    }

    public double? StackTopAt(int x, int y, int? excludeItemId = null)
    {
        lock (_furnitureLock)
        {
            return StackTopUnlocked(x, y, excludeItemId);
        }
    }

    private double? StackTopUnlocked(int x, int y, int? excludeItemId = null)
    {
        double top = Grid.HeightAt(x, y);
        foreach (RoomFloorItem item in ItemsAtUnlocked(x, y))
        {
            if (item.Id == excludeItemId)
            {
                continue;
            }

            if (!item.IsStackable)
            {
                return null;
            }

            top = Math.Max(top, item.SurfaceZ);
        }

        return Math.Round(top, 4);
    }

    private IReadOnlyList<RoomFloorItem> ItemsAtUnlocked(int x, int y) =>
        _itemsByTile.TryGetValue((x, y), out List<RoomFloorItem>? items) ? items : [];

    public void RemoveFloorItem(int itemId)
    {
        lock (_furnitureLock)
        {
            if (_floorItems.Remove(itemId))
            {
                RebuildTileIndex();
            }

            ForgetPetItem(itemId);
        }

        StopPlaybackOf(itemId);
    }

    public IReadOnlyList<RoomFloorItem> FloorItemsAt(int x, int y)
    {
        lock (_furnitureLock)
        {
            return ItemsAtUnlocked(x, y).ToArray();
        }
    }

    public double SurfaceHeightAt(int x, int y)
    {
        lock (_furnitureLock)
        {
            if (_topItemByTile.TryGetValue((x, y), out RoomFloorItem? top))
            {
                if (top.IsSittable || (top.IsBed && IsBedLayTile(top, x, y)))
                {
                    return top.Z;
                }

                if (top.IsWalkable)
                {
                    return top.SurfaceZ;
                }
            }
        }

        return Grid.HeightAt(x, y);
    }

    public HashSet<(int X, int Y)> FurnitureBlockedTiles(double walkerZ)
    {
        var blocked = new HashSet<(int X, int Y)>();
        lock (_furnitureLock)
        {
            foreach ((var tile, RoomFloorItem top) in _topItemByTile)
            {
                if (ClassifyUnlocked(top, tile.X, tile.Y, walkerZ) == TilePassability.Blocked)
                {
                    blocked.Add(tile);
                }
            }
        }

        return blocked;
    }

    public HashSet<(int X, int Y)> FurnitureGoalOnlyTiles()
    {
        var goalOnly = new HashSet<(int X, int Y)>();
        lock (_furnitureLock)
        {
            foreach ((var tile, RoomFloorItem top) in _topItemByTile)
            {
                if (IsRestingSpot(top, tile.X, tile.Y))
                {
                    goalOnly.Add(tile);
                }
            }
        }

        return goalOnly;
    }

    public bool IsFurniturePassable(int x, int y, double fromZ, bool isGoal)
    {
        lock (_furnitureLock)
        {
            if (!_topItemByTile.TryGetValue((x, y), out RoomFloorItem? top))
            {
                return true;
            }

            TilePassability passability = ClassifyUnlocked(top, x, y, fromZ);
            return passability == TilePassability.Open || (passability == TilePassability.GoalOnly && isGoal);
        }
    }

    private enum TilePassability { Open, GoalOnly, Blocked }

    private TilePassability ClassifyUnlocked(RoomFloorItem top, int x, int y, double fromZ)
    {
        if (IsRestingSpot(top, x, y))
        {
            return IsWithinReach(top.Z, x, y, fromZ) ? TilePassability.GoalOnly : TilePassability.Blocked;
        }

        if (top.IsWalkable && !top.IsBed)
        {
            return IsWithinReach(top.SurfaceZ, x, y, fromZ) ? TilePassability.Open : TilePassability.Blocked;
        }

        return TilePassability.Blocked;
    }

    private static bool IsRestingSpot(RoomFloorItem top, int x, int y) => top.IsSittable || (top.IsBed && IsBedLayTile(top, x, y));

    private bool IsWithinReach(double footingZ, int x, int y, double fromZ) =>
        footingZ - fromZ <= MaxClimbHeight || footingZ - Grid.HeightAt(x, y) <= MaxClimbHeight;

    private static bool IsBedLayTile(RoomFloorItem bed, int x, int y)
    {
        foreach ((int X, int Y) tile in bed.LayTiles())
        {
            if (tile.X == x && tile.Y == y)
            {
                return true;
            }
        }

        return false;
    }

    public (int X, int Y)? RedirectToBedLayTile(int x, int y, long moverSessionId)
    {
        RoomFloorItem? bed;
        lock (_furnitureLock)
        {
            bed = _floorItems.Values.FirstOrDefault(i => i.IsBed && i.OccupiedTiles().Contains((x, y)));
        }

        if (bed is null)
        {
            return null;
        }

        List<(int X, int Y)> layTiles = bed.LayTiles().ToList();
        if (layTiles.Contains((x, y)))
        {
            return null;
        }

        HashSet<(int X, int Y)> taken = OccupiedTiles(moverSessionId);
        List<(int X, int Y)> free = layTiles.Where(t => !taken.Contains(t)).ToList();
        IEnumerable<(int X, int Y)> candidates = free.Count > 0 ? free : layTiles;

        return candidates.OrderBy(t => Math.Abs(t.X - x) + Math.Abs(t.Y - y)).First();
    }

    public bool TeleportActor(RoomActor actor, int x, int y, double? z = null)
    {
        bool isDoor = x == Data.DoorX && y == Data.DoorY;
        if (!Grid.IsWalkable(x, y) && !isDoor)
        {
            return false;
        }

        lock (actor.Lock)
        {
            if (actor.X == x && actor.Y == y && !actor.IsWalking)
            {
                return false;
            }

            actor.IsWalking = false;
            actor.Path = null;
            actor.PathIndex = 0;
            actor.Goal = null;
            actor.PendingStep = null;
            actor.Posture = ActorPosture.Stand;
            actor.PostureHeight = 0;
            actor.X = x;
            actor.Y = y;

            if (z is { } height)
            {
                actor.Z = height;
            }
            else if (isDoor && !Grid.IsWalkable(x, y))
            {
                actor.Z = Data.DoorZ;
            }
            else
            {
                ApplyTilePosture(actor);
                if (actor.Posture == ActorPosture.Stand)
                {
                    actor.Z = TopSurfaceAt(x, y);
                }
            }
        }

        return true;
    }

    private double TopSurfaceAt(int x, int y)
    {
        lock (_furnitureLock)
        {
            return _topItemByTile.TryGetValue((x, y), out RoomFloorItem? top) ? Math.Max(top.SurfaceZ, Grid.HeightAt(x, y)) : Grid.HeightAt(x, y);
        }
    }

    public bool ApplyTilePosture(RoomActor actor)
    {
        RoomFloorItem? top;
        lock (_furnitureLock)
        {
            _topItemByTile.TryGetValue((actor.X, actor.Y), out top);
        }

        ActorPosture posture = ActorPosture.Stand;
        double z = Grid.HeightAt(actor.X, actor.Y);
        double postureHeight = 0;
        int? faceDir = null;

        if (top is not null)
        {
            if (top.IsBed && IsBedLayTile(top, actor.X, actor.Y))
            {
                posture = ActorPosture.Lay;
                z = top.Z;
                postureHeight = top.StackHeight;
                faceDir = (top.Rotation / 45) % 8;
            }
            else if (top.IsSittable)
            {
                posture = ActorPosture.Sit;
                z = top.Z;
                postureHeight = top.StackHeight;
                faceDir = (top.Rotation / 45) % 8;
            }
            else if (top.IsWalkable)
            {
                z = top.SurfaceZ;
            }
        }

        bool changed = actor.Posture != posture || Math.Abs(actor.Z - z) > 0.001;
        actor.Posture = posture;
        actor.PostureHeight = postureHeight;
        actor.Z = z;

        if (faceDir is { } dir && (actor.RotBody != dir || actor.RotHead != dir))
        {
            actor.RotBody = dir;
            actor.RotHead = dir;
            changed = true;
        }

        return changed;
    }

    private void RebuildTileIndex()
    {
        _stackingChanged = true;

        var itemsByTile = new Dictionary<(int X, int Y), List<RoomFloorItem>>();
        foreach (RoomFloorItem item in _floorItems.Values)
        {
            foreach ((int X, int Y) tile in item.OccupiedTiles())
            {
                if (!itemsByTile.TryGetValue(tile, out List<RoomFloorItem>? items))
                {
                    items = [];
                    itemsByTile[tile] = items;
                }

                items.Add(item);
            }
        }

        var topItemByTile = new Dictionary<(int X, int Y), RoomFloorItem>(itemsByTile.Count);
        var rollers = new List<RoomFloorItem>();
        foreach (((int X, int Y) tile, List<RoomFloorItem> items) in itemsByTile)
        {
            items.Sort(static (a, b) => a.Z.CompareTo(b.Z));

            RoomFloorItem top = items[0];
            foreach (RoomFloorItem item in items)
            {
                if (item.SurfaceZ >= top.SurfaceZ)
                {
                    top = item;
                }
            }

            topItemByTile[tile] = top;

            if (items.LastOrDefault(static i => i.IsRoller) is { IsSingleTile: true } roller)
            {
                rollers.Add(roller);
            }
        }

        _itemsByTile = itemsByTile;
        _topItemByTile = topItemByTile;
        _rollers = rollers;
    }

    public bool AdvanceMovement(out List<RoomActor> changed)
    {
        changed = new List<RoomActor>();
        bool needsFurtherTicks = false;

        lock (_movementLock)
        {
            foreach (RoomActor actor in _actors.Values)
            {
                lock (actor.Lock)
                {
                    bool actorChanged = false;

                    if (actor.PendingStep is { } pending)
                    {
                        actor.X = pending.X;
                        actor.Y = pending.Y;
                        actor.Z = pending.Z;
                        actor.PendingStep = null;
                        actorChanged = true;

                        if (!TryWarp(actor))
                        {
                            TryTakeExit(actor);
                            TryReachBusTile(actor);
                        }

                        if (!actor.IsWalking)
                        {
                            if (PublicExitTiles is null && actor.Pet is null && actor.Goal == (actor.X, actor.Y) && Grid.IsDoor(actor.X, actor.Y))
                            {
                                _walkedOut.Enqueue(actor);
                            }

                            actor.Goal = null;
                            actorChanged |= ApplyTilePosture(actor);

                            TryEnterLockerRoom(actor);
                        }
                    }

                    if (actor.IsWalking)
                    {
                        actorChanged |= AdvanceWalk(actor);
                    }

                    actorChanged |= UpdateSwimming(actor);

                    if (actorChanged)
                    {
                        changed.Add(actor);
                    }

                    needsFurtherTicks |= actor.IsWalking || actor.PendingStep is not null;
                }
            }
        }

        return needsFurtherTicks;
    }

    public bool? TryToggleGate(int itemId)
    {
        lock (_movementLock)
        {
            if (GetFloorItem(itemId) is not { IsGate: true } gate)
            {
                return null;
            }

            HashSet<(int X, int Y)> tiles = gate.OccupiedTiles().ToHashSet();
            foreach (RoomActor actor in _actors.Values)
            {
                lock (actor.Lock)
                {
                    if (tiles.Contains((actor.X, actor.Y)) || (actor.PendingStep is { } step && tiles.Contains((step.X, step.Y))))
                    {
                        return null;
                    }
                }
            }

            lock (_furnitureLock)
            {
                if (!_floorItems.ContainsKey(itemId)
                    || tiles.Any(tile => !_topItemByTile.TryGetValue(tile, out RoomFloorItem? top) || top.Id != itemId))
                {
                    return null;
                }

                gate.IsGateOpen = !gate.IsGateOpen;
                return gate.IsGateOpen;
            }
        }
    }

    public bool TryStartWalk(RoomActor actor, int goalX, int goalY) => TryStartWalk(actor, goalX, goalY, kick: false);

    private bool TryStartWalk(RoomActor actor, int goalX, int goalY, bool kick)
    {
        (HashSet<(int X, int Y)> occupied, HashSet<(int X, int Y)>? others) = ObstaclesFor(actor);

        lock (actor.Lock)
        {
            if (kick)
            {
                actor.KickTicksLeft ??= KickTicks;
            }
            else if (actor.IsKicked)
            {
                return false;
            }

            if (actor.IsHeldByFurniture)
            {
                return false;
            }

            (int startX, int startY, double startZ) = actor.PendingStep is { } pending
                ? (pending.X, pending.Y, pending.Z)
                : (actor.X, actor.Y, actor.Z);

            if (goalX == startX && goalY == startY)
            {
                return false;
            }

            occupied.UnionWith(FurnitureBlockedTiles(startZ));

            IReadOnlyList<(int X, int Y)>? path = AStarPathfinder.FindPath(
                Grid, occupied, (startX, startY), (goalX, goalY), FurnitureGoalOnlyTiles(), blockedAsGoal: others,
                allowedTiles: actor.Bot?.Definition.WalkableArea, allowDiagonal: actor.Bot?.Definition.AllowDiagonal ?? true);

            if (path is not { Count: > 1 })
            {
                return false;
            }

            actor.Posture = ActorPosture.Stand;
            actor.PostureHeight = 0;
            actor.Path = path.Skip(1).ToList();
            actor.PathIndex = 0;
            actor.Goal = (goalX, goalY);
            actor.IsWalking = true;
        }

        return true;
    }

    public bool CanStartTeleport(RoomActor actor, int teleporterItemId) => TryStartTeleportCore(actor, teleporterItemId, null);

    public bool TryStartTeleport(RoomActor actor, int teleporterItemId, TeleportDestination destination) =>
        TryStartTeleportCore(actor, teleporterItemId, destination);

    private bool TryStartTeleportCore(RoomActor actor, int teleporterItemId, TeleportDestination? destination)
    {
        RoomFloorItem? teleporter = GetFloorItem(teleporterItemId);
        if (teleporter is null || teleporter.InteractionLogic != FurnitureInteractionLogic.Teleport)
        {
            return false;
        }

        (int frontX, int frontY) = teleporter.SquareInFront();

        lock (_teleportLock)
        {
            if (_teleporterSequences.ContainsKey(teleporterItemId))
            {
                return false;
            }

            lock (actor.Lock)
            {
                if (actor.IsHeldByFurniture || actor.IsKicked || actor.IsWalking || actor.PendingStep is not null
                    || actor.X != frontX || actor.Y != frontY
                    || Math.Abs(teleporter.Z - actor.Z) > MaxClimbHeight)
                {
                    return false;
                }

                if (destination is { } target)
                {
                    var sequence = TeleporterSequence.Departure(teleporterItemId, actor.SessionId, target);
                    _teleporterSequences[teleporterItemId] = sequence;
                    actor.ActiveTeleport = sequence;
                }
            }
        }

        return true;
    }

    public bool CanArriveAt(RoomFloorItem teleporter, long arrivingSessionId)
    {
        if (teleporter.InteractionLogic != FurnitureInteractionLogic.Teleport)
        {
            return false;
        }

        lock (_teleportLock)
        {
            if (_teleporterSequences.ContainsKey(teleporter.Id))
            {
                return false;
            }
        }

        return !OccupiedTiles(arrivingSessionId).Contains((teleporter.X, teleporter.Y));
    }

    public bool TryBeginTeleportArrival(RoomActor actor, int teleporterItemId)
    {
        lock (_teleportLock)
        {
            if (_teleporterSequences.ContainsKey(teleporterItemId))
            {
                return false;
            }

            RegisterTeleporterSequence(TeleporterSequence.Arrival(teleporterItemId, actor.SessionId), actor);
        }

        return true;
    }

    internal void RegisterTeleporterSequence(TeleporterSequence sequence, RoomActor actor)
    {
        lock (_teleportLock)
        {
            _teleporterSequences[sequence.TeleporterItemId] = sequence;
            lock (actor.Lock)
            {
                actor.ActiveTeleport = sequence;
            }
        }
    }

    public TeleporterTickResult AdvanceTeleporters(List<RoomActor> changed)
    {
        var result = new TeleporterTickResult();

        lock (_movementLock)
        lock (_teleportLock)
        {
            if (_teleporterSequences.Count == 0)
            {
                return result;
            }

            foreach (TeleporterSequence sequence in _teleporterSequences.Values.ToList())
            {
                sequence.Advance(this, changed, result);
            }

            foreach (TeleporterSequence finished in _teleporterSequences.Values.Where(s => s.IsDone).ToList())
            {
                _teleporterSequences.Remove(finished.TeleporterItemId);
            }

            result.NeedsFurtherTicks |= _teleporterSequences.Count > 0;
        }

        return result;
    }

    public bool IsRolling(int itemId)
    {
        lock (_rollLock)
        {
            return _rolls.ContainsKey(itemId);
        }
    }

    public bool TryStartRoll(int itemId, IReadOnlyList<RollStep> steps)
    {
        if (steps.Count == 0 || !IsPlaced(itemId))
        {
            return false;
        }

        lock (_rollLock)
        {
            return _rolls.TryAdd(itemId, new FurnitureRollSequence(itemId, steps));
        }
    }

    public bool TryStopRoll(int itemId, int minStepsShown)
    {
        lock (_rollLock)
        {
            return _rolls.TryGetValue(itemId, out FurnitureRollSequence? roll) && roll.StepsShown >= minStepsShown && _rolls.Remove(itemId);
        }
    }

    public List<FurnitureStateChange> AdvanceRolls(out bool needsFurtherTicks)
    {
        var changes = new List<FurnitureStateChange>();

        lock (_rollLock)
        {
            foreach (FurnitureRollSequence roll in _rolls.Values.ToList())
            {
                if (!IsPlaced(roll.ItemId))
                {
                    _rolls.Remove(roll.ItemId);
                    continue;
                }

                roll.Advance(changes);
                if (roll.IsDone)
                {
                    _rolls.Remove(roll.ItemId);
                }
            }

            needsFurtherTicks = _rolls.Count > 0;
        }

        return changes;
    }

    private bool IsPlaced(int itemId)
    {
        lock (_furnitureLock)
        {
            return _floorItems.ContainsKey(itemId) || _wallItems.ContainsKey(itemId);
        }
    }

    public IReadOnlyList<RoomActor> TakeActorsWalkedOut()
    {
        if (_walkedOut.IsEmpty)
        {
            return [];
        }

        var actors = new List<RoomActor>();
        while (_walkedOut.TryDequeue(out RoomActor? actor))
        {
            actors.Add(actor);
        }

        return actors;
    }

    private bool AdvanceWalk(RoomActor actor)
    {
        if (actor.Path is null || actor.PathIndex >= actor.Path.Count)
        {
            actor.IsWalking = false;
            return false;
        }

        (int nextX, int nextY) = actor.Path[actor.PathIndex];

        if (!IsStepStillOpen(actor, nextX, nextY))
        {
            (HashSet<(int X, int Y)> blocked, HashSet<(int X, int Y)>? others) = ObstaclesFor(actor);
            blocked.UnionWith(FurnitureBlockedTiles(actor.Z));

            IReadOnlyList<(int X, int Y)>? replan = actor.Goal is { } goal
                ? AStarPathfinder.FindPath(
                    Grid, blocked, (actor.X, actor.Y), goal, FurnitureGoalOnlyTiles(), blockedAsGoal: others,
                    allowedTiles: actor.Bot?.Definition.WalkableArea, allowDiagonal: actor.Bot?.Definition.AllowDiagonal ?? true)
                : null;

            if (replan is { Count: > 1 })
            {
                actor.Path = replan.Skip(1).ToList();
                actor.PathIndex = 0;
                (nextX, nextY) = actor.Path[0];
            }
            else
            {
                actor.IsWalking = false;
                actor.Path = null;
                return true;
            }
        }

        double nextZ = SurfaceHeightAt(nextX, nextY);
        actor.RotBody = RotationHelper.Calculate(actor.X, actor.Y, nextX, nextY);
        actor.RotHead = actor.RotBody;
        actor.PendingStep = (nextX, nextY, nextZ);
        actor.PathIndex++;

        if (actor.PathIndex >= actor.Path.Count)
        {
            actor.IsWalking = false;
        }

        return true;
    }

    private bool IsStepStillOpen(RoomActor actor, int x, int y)
    {
        bool isGoal = actor.Goal is { } goal && goal == (x, y);
        bool steppable = Grid.IsWalkable(x, y) || (isGoal && Grid.IsWalkGoal(x, y));
        if (!steppable || !IsFurniturePassable(x, y, actor.Z, isGoal) || IsKeptOutOfWater(actor, x, y))
        {
            return false;
        }

        if (Data.AllowWalkthrough && !isGoal)
        {
            return true;
        }

        foreach (RoomActor other in _actors.Values)
        {
            if (other.SessionId == actor.SessionId)
            {
                continue;
            }

            lock (other.Lock)
            {
                if (other.X == x && other.Y == y)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
