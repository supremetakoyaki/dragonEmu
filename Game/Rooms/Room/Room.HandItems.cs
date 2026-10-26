namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    public const int HandItemTicks = 240;

    public static void GiveHandItem(RoomActor actor, int itemId)
    {
        lock (actor.Lock)
        {
            actor.CarryItemId = itemId;
            actor.CarryTicksLeft = itemId > 0 ? HandItemTicks : 0;
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
                if (actor.CarryItemId <= 0)
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
