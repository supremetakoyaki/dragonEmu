using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    private readonly object _oneWayDoorLock = new();

    private readonly Dictionary<int, OneWayDoorPassage> _oneWayDoorPassages = new();

    private readonly Dictionary<long, int> _oneWayDoorApproaches = new();

    public bool TryEnterOneWayDoor(RoomActor actor, RoomFloorItem door)
    {
        if (door.LogicClass != FurnitureLogicClass.OneWayDoor || GetFloorItem(door.Id) is null)
        {
            return false;
        }

        (int X, int Y) front = door.SquareInFront();
        bool there;
        lock (actor.Lock)
        {
            if (actor.IsHeldByFurniture || actor.IsKicked)
            {
                return false;
            }

            there = IsIdleOn(actor, front);
        }

        if (!there && !TryStartWalk(actor, front.X, front.Y) && !IsHeadingTo(actor, front))
        {
            return false;
        }

        lock (_oneWayDoorLock)
        {
            _oneWayDoorApproaches[actor.SessionId] = door.Id;
        }

        return true;
    }

    public List<OneWayDoorStatusChange> AdvanceOneWayDoors(List<RoomActor> changed, out bool needsFurtherTicks)
    {
        lock (_movementLock)
        {
            lock (_oneWayDoorLock)
            {
                var statusChanges = new List<OneWayDoorStatusChange>();
                if (_oneWayDoorApproaches.Count == 0 && _oneWayDoorPassages.Count == 0)
                {
                    needsFurtherTicks = false;
                    return statusChanges;
                }

                foreach ((long sessionId, int doorId) in _oneWayDoorApproaches.ToList())
                {
                    if (!AdvanceApproach(sessionId, doorId))
                    {
                        _oneWayDoorApproaches.Remove(sessionId);
                    }
                }

                foreach (OneWayDoorPassage passage in _oneWayDoorPassages.Values.ToList())
                {
                    passage.Advance(this, changed, statusChanges);
                    if (passage.IsDone)
                    {
                        _oneWayDoorPassages.Remove(passage.DoorItemId);
                    }
                }

                needsFurtherTicks = _oneWayDoorApproaches.Count > 0 || _oneWayDoorPassages.Count > 0;
                return statusChanges;
            }
        }
    }

    private bool AdvanceApproach(long sessionId, int doorId)
    {
        if (GetFloorItem(doorId) is not { LogicClass: FurnitureLogicClass.OneWayDoor } door
            || !TryGetActor(sessionId, out RoomActor? actor))
        {
            return false;
        }

        (int X, int Y) front = door.SquareInFront();
        lock (actor!.Lock)
        {
            if (actor.IsHeldByFurniture)
            {
                return false;
            }

            if (!IsIdleOn(actor, front))
            {
                return IsHeadingToCore(actor, front);
            }

            if (Math.Abs(door.Z - actor.Z) > MaxClimbHeight)
            {
                return false;
            }
        }

        if (_oneWayDoorPassages.ContainsKey(doorId)
            || IsTileTaken(door.X, door.Y, sessionId)
            || !IsFreeToStepOnto(door.SquareBehind(), door.Z, sessionId))
        {
            return true;
        }

        lock (actor.Lock)
        {
            if (!IsIdleOn(actor, front))
            {
                return IsHeadingToCore(actor, front);
            }

            var passage = new OneWayDoorPassage(doorId, sessionId);
            _oneWayDoorPassages[doorId] = passage;
            actor.ActiveOneWayDoor = passage;
            return false;
        }
    }

    internal bool IsFreeToStepOnto((int X, int Y) tile, double fromZ, long sessionId) =>
        Grid.IsWalkable(tile.X, tile.Y)
        && IsFurniturePassable(tile.X, tile.Y, fromZ, isGoal: true)
        && !IsTileTaken(tile.X, tile.Y, sessionId);

    internal bool IsTileTaken(int x, int y, long excludeSessionId)
    {
        foreach (RoomActor other in _actors.Values)
        {
            if (other.SessionId == excludeSessionId)
            {
                continue;
            }

            lock (other.Lock)
            {
                if ((other.X == x && other.Y == y) || (other.PendingStep is { } step && step.X == x && step.Y == y))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsHeadingTo(RoomActor actor, (int X, int Y) tile)
    {
        lock (actor.Lock)
        {
            return IsHeadingToCore(actor, tile);
        }
    }

    private static bool IsHeadingToCore(RoomActor actor, (int X, int Y) tile) =>
        (actor.IsWalking || actor.PendingStep is not null) && actor.Goal == tile;

    private static bool IsIdleOn(RoomActor actor, (int X, int Y) tile) =>
        actor.X == tile.X && actor.Y == tile.Y && !actor.IsWalking && actor.PendingStep is null;
}
