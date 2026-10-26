using DragonEmu.Game.Rooms.Pathfinding;

namespace DragonEmu.Game.Rooms;

public readonly record struct OneWayDoorStatusChange(int ItemId, int Status);

public sealed class OneWayDoorPassage
{
    public const int Closed = 0;
    public const int Open = 1;

    private enum Stage
    {
        Opening,
        Inside,
        Leaving,
        Done,
    }

    private Stage _stage = Stage.Opening;
    private bool _opened;

    internal OneWayDoorPassage(int doorItemId, long sessionId)
    {
        DoorItemId = doorItemId;
        SessionId = sessionId;
    }

    public int DoorItemId { get; }

    public long SessionId { get; }

    public bool IsDone => _stage == Stage.Done;

    internal void Advance(Room room, List<RoomActor> changed, List<OneWayDoorStatusChange> statusChanges)
    {
        RoomFloorItem? door = room.GetFloorItem(DoorItemId);
        room.TryGetActor(SessionId, out RoomActor? actor);

        if (door is null)
        {
            Release(actor);
            _stage = Stage.Done;
            return;
        }

        if (actor is null)
        {
            Close(door, null, statusChanges);
            return;
        }

        switch (_stage)
        {
            case Stage.Opening:
                if (!IsStandingOn(actor, door.SquareInFront())
                    || room.IsTileTaken(door.X, door.Y, SessionId)
                    || !room.IsFreeToStepOnto(door.SquareBehind(), door.Z, SessionId))
                {
                    Close(door, actor, statusChanges);
                    return;
                }

                lock (actor.Lock)
                {
                    actor.Posture = ActorPosture.Stand;
                    actor.PostureHeight = 0;
                }

                Step(actor, (door.X, door.Y), door.Z, changed);
                statusChanges.Add(new OneWayDoorStatusChange(door.Id, Open));
                _opened = true;
                _stage = Stage.Inside;
                break;

            case Stage.Inside:
                if (!IsStandingOn(actor, (door.X, door.Y)))
                {
                    Close(door, actor, statusChanges);
                    return;
                }

                lock (actor.Lock)
                {
                    actor.Z = door.Z;
                }

                (int X, int Y)? way = room.IsFreeToStepOnto(door.SquareBehind(), door.Z, SessionId) ? door.SquareBehind()
                    : room.IsFreeToStepOnto(door.SquareInFront(), door.Z, SessionId) ? door.SquareInFront()
                    : null;
                if (way is not { } tile)
                {
                    Close(door, actor, statusChanges);
                    return;
                }

                Step(actor, tile, room.SurfaceHeightAt(tile.X, tile.Y), changed);
                _stage = Stage.Leaving;
                break;

            case Stage.Leaving:
                Close(door, actor, statusChanges);
                break;
        }
    }

    private static void Step(RoomActor actor, (int X, int Y) tile, double z, List<RoomActor> changed)
    {
        lock (actor.Lock)
        {
            actor.RotBody = RotationHelper.Calculate(actor.X, actor.Y, tile.X, tile.Y);
            actor.RotHead = actor.RotBody;
            actor.PendingStep = (tile.X, tile.Y, z);
        }

        if (!changed.Contains(actor))
        {
            changed.Add(actor);
        }
    }

    private static bool IsStandingOn(RoomActor actor, (int X, int Y) tile)
    {
        lock (actor.Lock)
        {
            return actor.X == tile.X && actor.Y == tile.Y && actor.PendingStep is null && !actor.IsWalking;
        }
    }

    private void Close(RoomFloorItem door, RoomActor? actor, List<OneWayDoorStatusChange> statusChanges)
    {
        if (_opened)
        {
            statusChanges.Add(new OneWayDoorStatusChange(door.Id, Closed));
            _opened = false;
        }

        Release(actor);
        _stage = Stage.Done;
    }

    private void Release(RoomActor? actor)
    {
        if (actor is null)
        {
            return;
        }

        lock (actor.Lock)
        {
            if (ReferenceEquals(actor.ActiveOneWayDoor, this))
            {
                actor.ActiveOneWayDoor = null;
            }
        }
    }
}
