using DragonEmu.Game.Rooms.Pathfinding;

namespace DragonEmu.Game.Rooms;

public enum TeleportDestinationKind
{
    None,
    SameRoom,
    OtherRoom,
}

public readonly record struct TeleportDestination(TeleportDestinationKind Kind, int RoomId, int TeleporterItemId)
{
    public static TeleportDestination None { get; } = new(TeleportDestinationKind.None, 0, 0);

    public static TeleportDestination SameRoom(int roomId, int teleporterItemId) => new(TeleportDestinationKind.SameRoom, roomId, teleporterItemId);

    public static TeleportDestination OtherRoom(int roomId, int teleporterItemId) => new(TeleportDestinationKind.OtherRoom, roomId, teleporterItemId);
}

public readonly record struct TeleporterStateChange(int ItemId, int State);

public readonly record struct TeleporterDeparture(long SessionId, int RoomId, int TeleporterItemId);

public sealed class TeleporterTickResult
{
    public List<TeleporterStateChange> StateChanges { get; } = [];

    public List<TeleporterDeparture> Departures { get; } = [];

    public bool NeedsFurtherTicks { get; set; }
}

public sealed class TeleporterSequence
{
    internal const int NoPairWaitTicks = 2;

    private const int ExtraState2Ticks = 1;

    private enum Stage
    {
        Opening,
        WalkingIn,
        Lingering,
        Entered,
        Inside,
        Departed,
        Arrived,
        Emerging,
        Closing,
        Done,
    }

    private readonly TeleportDestination _destination;
    private Stage _stage;

    private int _stateIndex;

    private int _waitTicks;

    private TeleporterSequence(int teleporterItemId, long sessionId, TeleportDestination destination, Stage stage)
    {
        TeleporterItemId = teleporterItemId;
        SessionId = sessionId;
        _destination = destination;
        _stage = stage;
    }

    public int TeleporterItemId { get; }

    public long SessionId { get; }

    public bool IsDone => _stage == Stage.Done;

    internal static TeleporterSequence Departure(int teleporterItemId, long sessionId, TeleportDestination destination) =>
        new(teleporterItemId, sessionId, destination, Stage.Opening);

    internal static TeleporterSequence Arrival(int teleporterItemId, long sessionId) =>
        new(teleporterItemId, sessionId, TeleportDestination.None, Stage.Arrived);

    internal void Advance(Room room, List<RoomActor> changed, TeleporterTickResult result)
    {
        RoomFloorItem? teleporter = room.GetFloorItem(TeleporterItemId);
        room.TryGetActor(SessionId, out RoomActor? actor);

        if (teleporter is null)
        {
            Release(actor);
            _stage = Stage.Done;
            return;
        }

        if (actor is null)
        {
            Close(teleporter, null, result);
            return;
        }

        switch (_stage)
        {
            case Stage.Opening:
                if (!IsStandingOn(actor, teleporter.SquareInFront()))
                {
                    Close(teleporter, actor, result);
                    return;
                }

                lock (actor.Lock)
                {
                    actor.Posture = ActorPosture.Stand;
                    actor.PostureHeight = 0;
                    actor.RotBody = RotationHelper.Calculate(actor.X, actor.Y, teleporter.X, teleporter.Y);
                    actor.RotHead = actor.RotBody;
                    actor.PendingStep = (teleporter.X, teleporter.Y, teleporter.Z);
                }

                MarkChanged(changed, actor);
                SetState(teleporter, 1, result);
                result.NeedsFurtherTicks = true;
                _stage = Stage.WalkingIn;
                break;

            case Stage.WalkingIn:
                if (!IsStandingOn(actor, (teleporter.X, teleporter.Y)))
                {
                    Close(teleporter, actor, result);
                    return;
                }

                lock (actor.Lock)
                {
                    actor.Z = teleporter.Z;
                }

                MarkChanged(changed, actor);

                if (_destination.Kind == TeleportDestinationKind.None)
                {
                    _waitTicks = NoPairWaitTicks;
                    _stage = Stage.Lingering;
                    break;
                }

                _stage = Stage.Entered;
                break;

            case Stage.Entered:
                if (!IsStandingOn(actor, (teleporter.X, teleporter.Y)))
                {
                    Close(teleporter, actor, result);
                    return;
                }

                SetState(teleporter, 2, result);
                _waitTicks = ExtraState2Ticks;
                _stage = Stage.Inside;
                break;

            case Stage.Lingering:
                if (!IsStandingOn(actor, (teleporter.X, teleporter.Y)))
                {
                    Close(teleporter, actor, result);
                    return;
                }

                if (--_waitTicks > 0)
                {
                    break;
                }

                if (StepOut(room, teleporter, actor, changed, result))
                {
                    _stage = Stage.Closing;
                }
                else
                {
                    Close(teleporter, actor, result);
                }

                break;

            case Stage.Inside:
                if (!IsStandingOn(actor, (teleporter.X, teleporter.Y)))
                {
                    Close(teleporter, actor, result);
                    return;
                }

                if (_waitTicks-- > 0)
                {
                    break;
                }

                if (_destination.Kind == TeleportDestinationKind.OtherRoom)
                {
                    result.Departures.Add(new TeleporterDeparture(SessionId, _destination.RoomId, _destination.TeleporterItemId));
                    _stage = Stage.Departed;
                    break;
                }

                RoomFloorItem? pair = room.GetFloorItem(_destination.TeleporterItemId);
                if (pair is null || !room.CanArriveAt(pair, SessionId))
                {
                    StepOut(room, teleporter, actor, changed, result);
                    Close(teleporter, actor, result);
                    return;
                }

                lock (actor.Lock)
                {
                    actor.X = pair.X;
                    actor.Y = pair.Y;
                    actor.Z = pair.Z;
                    actor.RotBody = pair.Direction;
                    actor.RotHead = pair.Direction;
                    actor.PendingStep = null;
                    actor.Posture = ActorPosture.Stand;
                    actor.PostureHeight = 0;
                }

                MarkChanged(changed, actor);

                TeleporterSequence arrival = Arrival(pair.Id, SessionId);
                arrival.SetState(pair, 2, result);
                arrival._waitTicks = ExtraState2Ticks;
                arrival._stage = Stage.Emerging;
                room.RegisterTeleporterSequence(arrival, actor);
                _stage = Stage.Departed;
                break;

            case Stage.Departed:
            case Stage.Closing:
                Close(teleporter, actor, result);
                break;

            case Stage.Arrived:
                if (!IsStandingOn(actor, (teleporter.X, teleporter.Y)))
                {
                    Close(teleporter, actor, result);
                    return;
                }

                SetState(teleporter, 2, result);
                _waitTicks = ExtraState2Ticks;
                _stage = Stage.Emerging;
                break;

            case Stage.Emerging:
                if (!IsStandingOn(actor, (teleporter.X, teleporter.Y)))
                {
                    Close(teleporter, actor, result);
                    return;
                }

                if (_waitTicks-- > 0)
                {
                    break;
                }

                if (StepOut(room, teleporter, actor, changed, result))
                {
                    SetState(teleporter, 1, result);
                    _stage = Stage.Closing;
                }
                else
                {
                    Close(teleporter, actor, result);
                }

                break;
        }
    }

    private static bool StepOut(Room room, RoomFloorItem teleporter, RoomActor actor, List<RoomActor> changed, TeleporterTickResult result)
    {
        (int x, int y) = teleporter.SquareInFront();
        if (!room.Grid.IsWalkable(x, y)
            || !room.IsFurniturePassable(x, y, teleporter.Z, isGoal: true)
            || room.OccupiedTiles(actor.SessionId).Contains((x, y)))
        {
            return false;
        }

        double z = room.SurfaceHeightAt(x, y);
        lock (actor.Lock)
        {
            actor.RotBody = RotationHelper.Calculate(actor.X, actor.Y, x, y);
            actor.RotHead = actor.RotBody;
            actor.PendingStep = (x, y, z);
        }

        MarkChanged(changed, actor);
        result.NeedsFurtherTicks = true;
        return true;
    }

    private static bool IsStandingOn(RoomActor actor, (int X, int Y) tile)
    {
        lock (actor.Lock)
        {
            return actor.X == tile.X && actor.Y == tile.Y && actor.PendingStep is null && !actor.IsWalking;
        }
    }

    private static void MarkChanged(List<RoomActor> changed, RoomActor actor)
    {
        if (!changed.Contains(actor))
        {
            changed.Add(actor);
        }
    }

    private void SetState(RoomFloorItem teleporter, int index, TeleporterTickResult result)
    {
        _stateIndex = index;
        result.StateChanges.Add(new TeleporterStateChange(teleporter.Id, teleporter.StateValue(index)));
    }

    private void Close(RoomFloorItem teleporter, RoomActor? actor, TeleporterTickResult result)
    {
        if (_stateIndex != 0)
        {
            SetState(teleporter, 0, result);
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
            if (ReferenceEquals(actor.ActiveTeleport, this))
            {
                actor.ActiveTeleport = null;
            }
        }
    }
}
