using System.Collections.Concurrent;
using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    public const string PoolBoothClassname = "poolBooth";

    public const double PoolWaterMaxHeight = 3;

    public static readonly IReadOnlyList<string> SwimsuitColors =
    [
        "238,238,238", "250,56,49", "253,146,160", "42,199,210", "53,51,44", "239,255,146", "198,255,152", "255,146,90",
        "157,89,126", "182,243,255", "109,255,51", "51,120,201", "255,182,49", "223,161,233", "249,251,50", "202,175,143",
        "197,198,197", "71,98,61", "138,131,97", "255,140,51", "84,198,39", "30,108,153", "152,79,136", "119,200,255",
        "255,192,142", "60,75,135", "124,44,71", "215,255,227", "143,63,28", "255,99,147", "31,155,121", "253,255,51",
    ];

    public static string RandomSwimsuit(Random? random = null) =>
        $"ch=s02/{SwimsuitColors[(random ?? Random.Shared).Next(SwimsuitColors.Count)]}";

    public static bool IsSwimsuit(string? poolFigure) =>
        poolFigure is not null && poolFigure.StartsWith("ch=s", StringComparison.Ordinal) && poolFigure.IndexOf('/') is var slash and > 0
        && SwimsuitColors.Contains(poolFigure[(slash + 1)..]);

    private static readonly IReadOnlySet<(int X, int Y)> NoTiles = new HashSet<(int X, int Y)>();

    private readonly ConcurrentQueue<RoomActor> _enteredLockerRoom = new();
    private IReadOnlySet<(int X, int Y)> _poolWater = NoTiles;
    private IReadOnlySet<(int X, int Y)> _poolBooths = NoTiles;

    public bool IsPool { get; private set; }

    public IReadOnlySet<(int X, int Y)> PoolWater => _poolWater;

    private void LoadPool(IReadOnlyList<PublicRoomFurnitureItem> furniture)
    {
        _poolBooths = furniture.Where(item => item.Classname == PoolBoothClassname).Select(item => (item.X, item.Y)).ToHashSet();
        IsPool = _poolBooths.Count > 0 || furniture.Any(item => item.Classname is PoolEnterClassname or PoolExitClassname);
        if (!IsPool)
        {
            _poolWater = NoTiles;
            return;
        }

        double waterMaxHeight = Data.PoolWaterMaxHeight ?? PoolWaterMaxHeight;
        var water = new HashSet<(int X, int Y)>();
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                if (Grid.IsWalkable(x, y) && Grid.HeightAt(x, y) <= waterMaxHeight)
                {
                    water.Add((x, y));
                }
            }
        }

        _poolWater = water;
    }

    public bool IsInWater(RoomActor actor, int x, int y) => actor.HasSwimwear && _poolWater.Contains((x, y));

    private bool IsKeptOutOfWater(RoomActor actor, int x, int y) => !actor.HasSwimwear && _poolWater.Contains((x, y));

    private void TryEnterLockerRoom(RoomActor actor)
    {
        if (actor.Type == ActorType.Player && !actor.InLockerRoom && _poolBooths.Contains((actor.X, actor.Y)))
        {
            actor.InLockerRoom = true;
            _enteredLockerRoom.Enqueue(actor);
        }
    }

    private bool UpdateSwimming(RoomActor actor)
    {
        bool swimming = IsInWater(actor, actor.X, actor.Y);
        if (swimming == actor.IsSwimming)
        {
            return false;
        }

        actor.IsSwimming = swimming;
        return true;
    }

    public IReadOnlyList<RoomActor> TakeActorsInLockerRoom()
    {
        if (_enteredLockerRoom.IsEmpty)
        {
            return [];
        }

        var actors = new List<RoomActor>();
        while (_enteredLockerRoom.TryDequeue(out RoomActor? actor))
        {
            actors.Add(actor);
        }

        return actors;
    }

    public bool LeaveLockerRoom(RoomActor actor, string swimsuit)
    {
        lock (actor.Lock)
        {
            if (!actor.InLockerRoom)
            {
                return false;
            }

            actor.InLockerRoom = false;
            actor.Swimsuit = actor.HasSwimwear ? null : swimsuit;
            UpdateSwimming(actor);
            return true;
        }
    }
}
