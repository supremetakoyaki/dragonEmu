namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    public const int HandItemTicks = 240;

    private const int HeldUntilPutAway = -1;

    public event Action<Room, RoomActor>? HeldHandItemLost;

    public void GiveHandItem(RoomActor actor, int itemId, bool held = false)
    {
        bool heldLost;
        lock (actor.Lock)
        {
            heldLost = actor.CarryTicksLeft == HeldUntilPutAway && actor.CarryItemId != itemId;
            actor.CarryItemId = itemId;
            actor.CarryTicksLeft = itemId <= 0 ? 0 : held ? HeldUntilPutAway : HandItemTicks;
        }

        if (heldLost)
        {
            HeldHandItemLost?.Invoke(this, actor);
        }
    }

    public List<RoomActor> AdvanceHandItems(out bool anyoneStillHolding)
    {
        var putDown = new List<RoomActor>();
        anyoneStillHolding = false;

        foreach (RoomActor actor in _actors.Values)
        {
            lock (actor.Lock)
            {
                if (actor.CarryItemId <= 0 || actor.CarryTicksLeft == HeldUntilPutAway)
                {
                    continue;
                }

                if (--actor.CarryTicksLeft > 0)
                {
                    anyoneStillHolding = true;
                    continue;
                }

                actor.CarryItemId = 0;
                actor.CarryTicksLeft = 0;
                putDown.Add(actor);
            }
        }

        return putDown;
    }
}
