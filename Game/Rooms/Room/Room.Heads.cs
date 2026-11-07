using DragonEmu.Game.Rooms.Pathfinding;

namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    public const int HeadTurnTicks = 4;

    public const int TalkHeadRange = 4;

    public List<RoomActor> TurnHeadsToward(RoomActor speaker, bool shout)
    {
        int x, y;
        lock (speaker.Lock)
        {
            (x, y) = (speaker.X, speaker.Y);
        }

        var turned = new List<RoomActor>();
        foreach (RoomActor listener in _actors.Values)
        {
            if (listener == speaker)
            {
                continue;
            }

            lock (listener.Lock)
            {
                if (!CanTurnHead(listener) || (!shout && Math.Max(Math.Abs(listener.X - x), Math.Abs(listener.Y - y)) > TalkHeadRange)
                    || HeadToward(listener.X, listener.Y, listener.RotBody, x, y) is not { } head)
                {
                    continue;
                }

                if (head != listener.RotBody)
                {
                    listener.HeadTurnTicksLeft = HeadTurnTicks;
                }

                if (listener.RotHead != head)
                {
                    listener.RotHead = head;
                    turned.Add(listener);
                }
            }
        }

        return turned;
    }

    public List<RoomActor> AdvanceHeads(out bool anyStillTurned)
    {
        var back = new List<RoomActor>();
        anyStillTurned = false;

        foreach (RoomActor actor in _actors.Values)
        {
            lock (actor.Lock)
            {
                if (actor.HeadTurnTicksLeft <= 0)
                {
                    continue;
                }

                if (--actor.HeadTurnTicksLeft > 0)
                {
                    anyStillTurned = true;
                    continue;
                }

                if (actor.RotHead != actor.RotBody)
                {
                    actor.RotHead = actor.RotBody;
                    back.Add(actor);
                }
            }
        }

        return back;
    }

    public bool HeadsTurned => _actors.Values.Any(actor =>
    {
        lock (actor.Lock)
        {
            return actor.HeadTurnTicksLeft > 0;
        }
    });

    private static bool CanTurnHead(RoomActor listener) =>
        listener.Posture == ActorPosture.Stand && !listener.IsWalking && !listener.IsHeldByFurniture
        && listener.AsleepSince is null && listener.DanceId == 0 && listener.BotDanceTicksLeft == 0 && listener.EffectId == 0
        && listener.PetPosture is null;

    private static int? HeadToward(int x, int y, int body, int speakerX, int speakerY)
    {
        if (x == speakerX && y == speakerY)
        {
            return null;
        }

        int steps = (RotationHelper.Calculate(x, y, speakerX, speakerY) - body + 8) % 8;
        return steps switch
        {
            0 => body,
            < 4 => (body + 1) % 8,
            > 4 => (body + 7) % 8,
            _ => null,
        };
    }
}
