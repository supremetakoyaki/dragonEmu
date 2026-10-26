using DragonEmu.Game.Rooms;

namespace DragonEmu.Game.Furniture;

public sealed class OneWayDoorService
{
    private readonly RoomMovementScheduler _scheduler;

    public OneWayDoorService(RoomMovementScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public void Enter(Room room, RoomActor actor, RoomFloorItem door)
    {
        if (room.TryEnterOneWayDoor(actor, door))
        {
            _scheduler.MarkActive(room);
        }
    }
}
