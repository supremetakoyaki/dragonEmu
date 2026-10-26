namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    public const int KickTicks = 6;

    public bool StartKick(RoomActor actor)
    {
        lock (actor.Lock)
        {
            if (actor.IsKicked)
            {
                return false;
            }
        }

        TryStartWalk(actor, Data.DoorX, Data.DoorY, kick: true);
        return true;
    }

    public List<RoomActor> AdvanceKicks(out bool kicksUnderWay)
    {
        var due = new List<RoomActor>();
        kicksUnderWay = false;

        foreach (RoomActor actor in _actors.Values)
        {
            lock (actor.Lock)
            {
                if (actor.KickTicksLeft is not { } ticksLeft)
                {
                    continue;
                }

                bool stoppedShort = !actor.IsWalking && actor.PendingStep is null;
                if (stoppedShort || ticksLeft <= 1)
                {
                    actor.KickTicksLeft = 0;
                    due.Add(actor);
                    continue;
                }

                actor.KickTicksLeft = ticksLeft - 1;
                kicksUnderWay = true;
            }
        }

        return due;
    }
}
