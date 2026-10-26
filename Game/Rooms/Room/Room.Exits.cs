namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    private static readonly IReadOnlySet<(int X, int Y)> NoExitTiles = new HashSet<(int X, int Y)>();

    public IReadOnlySet<(int X, int Y)>? PublicExitTiles { get; set; }

    private bool TryTakeExit(RoomActor actor)
    {
        if (actor.Type != ActorType.Player || PublicExitTiles is not { } exits || !exits.Contains((actor.X, actor.Y)))
        {
            return false;
        }

        actor.IsWalking = false;
        actor.Path = null;
        actor.Goal = null;
        _walkedOut.Enqueue(actor);
        return true;
    }
}
