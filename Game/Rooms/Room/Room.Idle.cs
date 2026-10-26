namespace DragonEmu.Game.Rooms;

public sealed record IdleCheckResult(List<RoomActor> FellAsleep, List<RoomActor> SleptTooLong);

public sealed partial class Room
{
    public static bool WakeUp(RoomActor actor, DateTime utcNow)
    {
        lock (actor.Lock)
        {
            actor.LastActiveAt = utcNow;
            if (actor.AsleepSince is null)
            {
                return false;
            }

            actor.AsleepSince = null;
            actor.SleptTooLong = false;
            return true;
        }
    }

    public IdleCheckResult CheckIdle(DateTime utcNow, TimeSpan sleepAfter, TimeSpan? kickAfter)
    {
        var result = new IdleCheckResult([], []);
        foreach (RoomActor actor in _actors.Values)
        {
            if (actor.Type != ActorType.Player)
            {
                continue;
            }

            lock (actor.Lock)
            {
                if (actor.AsleepSince is null)
                {
                    if (utcNow - actor.LastActiveAt >= sleepAfter)
                    {
                        actor.AsleepSince = utcNow;
                        result.FellAsleep.Add(actor);
                    }
                }
                else if (kickAfter is { } kick && !actor.SleptTooLong && utcNow - actor.AsleepSince.Value >= kick)
                {
                    actor.SleptTooLong = true;
                    result.SleptTooLong.Add(actor);
                }
            }
        }

        return result;
    }
}
