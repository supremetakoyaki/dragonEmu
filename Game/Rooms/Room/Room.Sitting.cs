namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    public const double FloorSitHeight = 0.5;

    public const double FloorLayHeight = 0.0;

    public static bool TryRestWhereStanding(RoomActor actor, ActorPosture posture, out bool stoppedDancing)
    {
        lock (actor.Lock)
        {
            stoppedDancing = false;
            if (posture == ActorPosture.Stand
                || actor.IsWalking || actor.PendingStep is not null || actor.IsHeldByFurniture || actor.Posture != ActorPosture.Stand
                || actor.IsSwimming)
            {
                return false;
            }

            actor.Posture = posture;
            actor.PostureHeight = posture == ActorPosture.Sit ? FloorSitHeight : FloorLayHeight;
            actor.RotBody -= actor.RotBody % 2;
            actor.RotHead = actor.RotBody;

            stoppedDancing = actor.DanceId > 0;
            actor.DanceId = 0;
            return true;
        }
    }
}
