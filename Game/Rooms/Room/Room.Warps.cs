using System.Collections.Concurrent;
using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public sealed record WarpedActor(RoomActor Actor, PublicRoomWarpTile Warp, int Rotation);

public sealed partial class Room
{
    private static readonly IReadOnlyDictionary<(int X, int Y), PublicRoomWarpTile> NoWarpTiles =
        new Dictionary<(int X, int Y), PublicRoomWarpTile>();

    private readonly ConcurrentQueue<WarpedActor> _warped = new();

    public IReadOnlyDictionary<(int X, int Y), PublicRoomWarpTile> WarpTiles { get; set; } = NoWarpTiles;

    public IReadOnlyList<WarpedActor> TakeActorsWarped()
    {
        if (_warped.IsEmpty)
        {
            return [];
        }

        var warped = new List<WarpedActor>();
        while (_warped.TryDequeue(out WarpedActor? actor))
        {
            warped.Add(actor);
        }

        return warped;
    }

    private readonly ConcurrentQueue<RoomActor> _atBusTile = new();

    public (int X, int Y)? BusEntranceTile { get; set; }

    public IReadOnlyList<RoomActor> TakeActorsAtBusTile()
    {
        if (_atBusTile.IsEmpty)
        {
            return [];
        }

        var reached = new List<RoomActor>();
        while (_atBusTile.TryDequeue(out RoomActor? actor))
        {
            reached.Add(actor);
        }

        return reached;
    }

    private void TryReachBusTile(RoomActor actor)
    {
        if (actor.Type == ActorType.Player && BusEntranceTile is { } tile && (actor.X, actor.Y) == tile)
        {
            _atBusTile.Enqueue(actor);
        }
    }

    private bool TryWarp(RoomActor actor)
    {
        if (actor.Type != ActorType.Player || !WarpTiles.TryGetValue((actor.X, actor.Y), out PublicRoomWarpTile? warp))
        {
            return false;
        }

        actor.IsWalking = false;
        actor.Path = null;
        actor.Goal = null;
        _warped.Enqueue(new WarpedActor(actor, warp, actor.RotBody));
        return true;
    }
}
