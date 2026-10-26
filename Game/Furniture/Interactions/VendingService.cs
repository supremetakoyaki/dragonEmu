using DragonEmu.Game.Rooms;

namespace DragonEmu.Game.Furniture;

public sealed class VendingService
{
    private readonly RoomMovementScheduler _scheduler;

    public VendingService(RoomMovementScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public void Use(Room room, RoomActor actor, RoomFloorItem machine)
    {
        bool nextToIt;
        lock (actor.Lock)
        {
            nextToIt = machine.IsNextTo(actor.X, actor.Y) && !actor.IsWalking && actor.PendingStep is null;
        }

        if (!nextToIt)
        {
            (int frontX, int frontY) = machine.SquareInFront();
            if (room.TryStartWalk(actor, frontX, frontY))
            {
                _scheduler.MarkActive(room);
            }

            return;
        }

        int handItemId = machine.InteractionIds is { Count: > 0 } handItems ? handItems[Random.Shared.Next(handItems.Count)] : 0;
        if (room.TryStartVending(machine, actor, handItemId))
        {
            _scheduler.MarkActive(room);
        }
    }
}
