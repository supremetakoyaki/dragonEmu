using DragonEmu.Game.Rooms;

namespace DragonEmu.Game.Furniture;

public sealed class HockeyLightService
{
    private readonly RoomMovementScheduler _scheduler;

    public HockeyLightService(RoomMovementScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public void Use(Room room, RoomActor actor, RoomFloorItem light)
    {
        bool nextToIt;
        lock (actor.Lock)
        {
            nextToIt = light.IsNextTo(actor.X, actor.Y);
        }

        if (nextToIt && room.TryLightHockeyLight(light))
        {
            _scheduler.MarkActive(room);
        }
    }
}
