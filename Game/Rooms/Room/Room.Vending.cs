using DragonEmu.Game.Rooms.Pathfinding;

namespace DragonEmu.Game.Rooms;

public readonly record struct VendedHandItem(RoomActor Actor, int HandItemId);

public sealed record VendingTickResult(IReadOnlyList<FurnitureStateChange> StateChanges, IReadOnlyList<VendedHandItem> HandItems, bool NeedsFurtherTicks)
{
    public static VendingTickResult Idle { get; } = new([], [], false);
}

public sealed partial class Room
{
    private const string VendingIdle = "0";
    private const string VendingServing = "1";

    private readonly object _vendingLock = new();
    private readonly Dictionary<int, VendingUse> _vending = new();

    public bool TryStartVending(RoomFloorItem machine, RoomActor actor, int handItemId)
    {
        if (GetFloorItem(machine.Id) is null)
        {
            return false;
        }

        lock (_vendingLock)
        {
            return _vending.TryAdd(machine.Id, new VendingUse(actor.SessionId, handItemId));
        }
    }

    public VendingTickResult AdvanceVending(List<RoomActor> changed)
    {
        lock (_vendingLock)
        {
            if (_vending.Count == 0)
            {
                return VendingTickResult.Idle;
            }

            var stateChanges = new List<FurnitureStateChange>();
            var handItems = new List<VendedHandItem>();

            foreach ((int machineId, VendingUse use) in _vending.ToList())
            {
                if (GetFloorItem(machineId) is not { } machine)
                {
                    _vending.Remove(machineId);
                    continue;
                }

                use.Ticks++;
                _actors.TryGetValue(use.SessionId, out RoomActor? actor);

                switch (use.Ticks)
                {
                    case 1:
                        stateChanges.Add(new FurnitureStateChange(machineId, VendingServing));
                        if (actor is not null && FaceMachine(actor, machine) && !changed.Contains(actor))
                        {
                            changed.Add(actor);
                        }

                        break;

                    case 2:
                        if (actor is not null && use.HandItemId > 0)
                        {
                            GiveHandItem(actor, use.HandItemId);
                            handItems.Add(new VendedHandItem(actor, use.HandItemId));
                        }

                        break;

                    default:
                        stateChanges.Add(new FurnitureStateChange(machineId, VendingIdle));
                        _vending.Remove(machineId);
                        break;
                }
            }

            return new VendingTickResult(stateChanges, handItems, _vending.Count > 0);
        }
    }

    private static bool FaceMachine(RoomActor actor, RoomFloorItem machine)
    {
        lock (actor.Lock)
        {
            if (actor.IsWalking || actor.PendingStep is not null || actor.Posture != ActorPosture.Stand)
            {
                return false;
            }

            int rotation = RotationHelper.Calculate(actor.X, actor.Y, machine.X, machine.Y);
            if (actor.RotBody == rotation && actor.RotHead == rotation)
            {
                return false;
            }

            actor.RotBody = rotation;
            actor.RotHead = rotation;
            return true;
        }
    }

    private sealed class VendingUse(long sessionId, int handItemId)
    {
        public long SessionId { get; } = sessionId;

        public int HandItemId { get; } = handItemId;

        public int Ticks { get; set; }
    }
}
