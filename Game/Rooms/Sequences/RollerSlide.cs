namespace DragonEmu.Game.Rooms;

public readonly record struct SlidingItem(int ItemId, double FromZ, double ToZ);

public readonly record struct SlidingRider(
    int VirtualId, double FromZ, double ToZ, ActorPosture Posture = ActorPosture.Stand, double PostureHeight = 0, int RotHead = 0, int RotBody = 0)
{
    public bool IsResting => Posture != ActorPosture.Stand;
}

public sealed record RollerSlide(
    int RollerId, (int X, int Y) From, (int X, int Y) To, IReadOnlyList<SlidingItem> Items, SlidingRider? Rider);

public sealed record RollerTickResult(IReadOnlyList<RollerSlide> Slides, bool NeedsFurtherTicks)
{
    public static RollerTickResult Idle { get; } = new([], false);
}
