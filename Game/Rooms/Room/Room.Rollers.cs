using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    private const int SlideMilliseconds = 500;

    private const double HeightTolerance = 0.001;

    private readonly object _rollerLock = new();

    private readonly Dictionary<int, long> _rollerDueTicks = new();
    private readonly List<RoomActor> _ridersToSettle = [];
    private long _rollerTick;

    private List<RoomFloorItem> _rollers = [];
    private Dictionary<(int X, int Y), (int X, int Y)> _slidingStacks = new();
    private long _slidesShownUntil;
    private readonly HashSet<int> _rolledItemIds = new();
    private readonly HashSet<int> _unsavedPositions = new();

    public bool HasRollers
    {
        get
        {
            lock (_furnitureLock)
            {
                return _rollers.Count > 0;
            }
        }
    }

    public bool RollersHaveWork()
    {
        if (UserCount == 0)
        {
            return false;
        }

        HashSet<(int X, int Y)> standing = OccupiedTiles(excludeSessionId: -1);
        lock (_furnitureLock)
        {
            return _rollers.Any(roller => standing.Contains((roller.X, roller.Y)) || LoadOf(roller).Count > 0);
        }
    }

    public RollerTickResult AdvanceRollers(List<RoomActor> changed, bool furniFusion)
    {
        lock (_movementLock)
        lock (_rollerLock)
        {
            _rollerTick++;
            List<RoomActor> lastRiders = [.. _ridersToSettle];
            _ridersToSettle.Clear();

            if (UserCount == 0 || !HasRollers)
            {
                _rollerDueTicks.Clear();
                SettleRiders(lastRiders, changed);
                return RollerTickResult.Idle;
            }

            HashSet<int> teleportersInUse = TeleportersInUse();
            List<Occupant> occupants = SnapshotOccupants();

            List<RollerMove> moves;
            lock (_furnitureLock)
            {
                moves = PlanRolls(occupants, teleportersInUse, furniFusion);
                MoveLoads(moves);
            }

            var slides = new List<RollerSlide>(moves.Count);
            foreach (RollerMove move in moves)
            {
                if (FinishRoll(move) is { } slide)
                {
                    slides.Add(slide);
                }
            }

            lastRiders.RemoveAll(rider => slides.Any(slide => slide.Rider?.VirtualId == rider.VirtualId));
            SettleRiders(lastRiders, changed);

            return new RollerTickResult(slides, NeedsFurtherTicks: _rollerDueTicks.Count > 0 || _ridersToSettle.Count > 0);
        }
    }

    public (int X, int Y) PlacementTile(int x, int y)
    {
        lock (_furnitureLock)
        {
            return Environment.TickCount64 < _slidesShownUntil && _slidingStacks.TryGetValue((x, y), out (int X, int Y) to) ? to : (x, y);
        }
    }

    public IReadOnlyList<FloorItemPosition> TakeUnsavedFloorPositions()
    {
        lock (_furnitureLock)
        {
            if (_unsavedPositions.Count == 0)
            {
                return [];
            }

            var positions = new List<FloorItemPosition>(_unsavedPositions.Count);
            foreach (int itemId in _unsavedPositions)
            {
                if (_floorItems.TryGetValue(itemId, out RoomFloorItem? item))
                {
                    positions.Add(new FloorItemPosition(itemId, item.X, item.Y, item.Z));
                }
            }

            _unsavedPositions.Clear();
            return positions;
        }
    }

    public void MarkFloorPositionsUnsaved(IEnumerable<int> itemIds)
    {
        lock (_furnitureLock)
        {
            _unsavedPositions.UnionWith(itemIds);
        }
    }

    private void MarkPositionUnsavedIfRolled(int itemId)
    {
        if (_rolledItemIds.Contains(itemId))
        {
            _unsavedPositions.Add(itemId);
        }
    }

    private void SettleRiders(List<RoomActor> riders, List<RoomActor> changed)
    {
        foreach (RoomActor rider in riders)
        {
            if (_actors.ContainsKey(rider.SessionId) && !changed.Contains(rider))
            {
                changed.Add(rider);
            }
        }
    }

    private HashSet<int> TeleportersInUse()
    {
        lock (_teleportLock)
        {
            return _teleporterSequences.Keys.ToHashSet();
        }
    }

    private List<Occupant> SnapshotOccupants()
    {
        var occupants = new List<Occupant>(_actors.Count);
        foreach (RoomActor actor in _actors.Values)
        {
            lock (actor.Lock)
            {
                occupants.Add(new Occupant(
                    actor, (actor.X, actor.Y), actor.Z,
                    IsMoving: actor.IsWalking || actor.PendingStep is not null,
                    SteppingOnto: actor.PendingStep is { } step ? (step.X, step.Y) : null,
                    InTeleporter: actor.ActiveTeleport?.TeleporterItemId));
            }
        }

        return occupants;
    }

    private static bool IsFreeToRoll(
        RoomFloorItem roller, IReadOnlyList<RoomFloorItem> load, Occupant? rider, List<Occupant> occupants, HashSet<int> teleportersInUse)
    {
        if (rider is { } on && (on.IsMoving || (on.InTeleporter is { } teleporterId && !load.Any(item => item.Id == teleporterId))))
        {
            return false;
        }

        if (occupants.Any(occupant => occupant.SteppingOnto == (roller.X, roller.Y)))
        {
            return false;
        }

        return load.All(item => item.IsSingleTile && !item.IsPositionPending
            && (!teleportersInUse.Contains(item.Id) || rider?.InTeleporter == item.Id));
    }

    private List<RollerMove> PlanRolls(List<Occupant> occupants, HashSet<int> teleportersInUse, bool furniFusion)
    {
        int interval = Data.RollerIntervalTicks;
        var candidates = new List<RollerMove>();
        var present = new HashSet<int>();

        foreach (RoomFloorItem roller in _rollers)
        {
            present.Add(roller.Id);
            IReadOnlyList<RoomFloorItem> load = LoadOf(roller);
            Occupant? rider = OccupantOn(occupants, (roller.X, roller.Y));

            if ((load.Count == 0 && rider is not { IsMoving: false }) || (roller.Id < 0 && TopRollerAt(roller.SquareInFront()) is null))
            {
                _rollerDueTicks.Remove(roller.Id);
                continue;
            }

            if (!_rollerDueTicks.TryGetValue(roller.Id, out long dueTick))
            {
                _rollerDueTicks[roller.Id] = _rollerTick + interval;
                continue;
            }

            if (_rollerTick < dueTick)
            {
                continue;
            }

            _rollerDueTicks[roller.Id] = _rollerTick + interval;
            if (IsFreeToRoll(roller, load, rider, occupants, teleportersInUse))
            {
                candidates.Add(new RollerMove(roller, load, rider));
            }
        }

        foreach (int gone in _rollerDueTicks.Keys.Where(id => !present.Contains(id)).ToList())
        {
            _rollerDueTicks.Remove(gone);
        }

        List<RollerMove> moves = ResolveRolls(candidates, occupants, furniFusion);

        foreach (RollerMove move in moves)
        {
            _rollerDueTicks.Remove(move.Roller.Id);
        }

        foreach (RollerMove move in moves)
        {
            if (TopRollerAt(move.To) is { } next)
            {
                _rollerDueTicks[next.Id] = _rollerTick + interval;
            }
        }

        return moves;
    }

    private List<RollerMove> ResolveRolls(List<RollerMove> candidates, List<Occupant> occupants, bool furniFusion)
    {
        var going = new Dictionary<(int X, int Y), RollerMove>();
        var claimed = new HashSet<(int X, int Y)>();

        foreach (RollerMove move in candidates.OrderBy(move => move.Roller.Id))
        {
            if (!claimed.Contains(move.To) && FindLanding(move, furniFusion) is { } landing)
            {
                move.Landing = landing;
                going[move.From] = move;
                claimed.Add(move.To);
            }
        }

        bool dropped;
        do
        {
            dropped = false;
            foreach (RollerMove move in going.Values.ToList())
            {
                if (!IsDestinationClear(move, going, occupants))
                {
                    going.Remove(move.From);
                    dropped = true;
                }
            }
        }
        while (dropped);

        return going.Values.OrderBy(move => move.Roller.Id).ToList();
    }

    private double? FindLanding(RollerMove move, bool furniFusion)
    {
        (int x, int y) = move.To;
        if (!Grid.CanHoldFurniture(x, y))
        {
            return null;
        }

        RoomFloorItem? nextRoller = TopRollerAt(move.To);

        if (move.Load.Count == 0)
        {
            double riderZ = move.Rider!.Value.Z;
            bool reachable = nextRoller is not null
                ? IsWithinReach(nextRoller.SurfaceZ, x, y, riderZ)
                : IsFurniturePassable(x, y, riderZ, isGoal: true);
            return reachable ? riderZ : null;
        }

        if (nextRoller is not null)
        {
            return nextRoller.SurfaceZ;
        }

        return furniFusion && move.Rider is null ? Grid.HeightAt(x, y) : StackTopUnlocked(x, y);
    }

    private bool IsDestinationClear(RollerMove move, Dictionary<(int X, int Y), RollerMove> going, List<Occupant> occupants)
    {
        going.TryGetValue(move.To, out RollerMove? leaving);

        foreach (Occupant occupant in occupants)
        {
            if (occupant.SteppingOnto == move.To
                || (occupant.Tile == move.To && leaving?.Rider?.Actor != occupant.Actor))
            {
                return false;
            }
        }

        return leaving is not null || TopRollerAt(move.To) is not { } nextRoller || LoadOf(nextRoller).Count == 0;
    }

    private void MoveLoads(List<RollerMove> moves)
    {
        if (moves.Count == 0)
        {
            return;
        }

        var sliding = new Dictionary<(int X, int Y), (int X, int Y)>();

        foreach (RollerMove move in moves)
        {
            foreach (RoomFloorItem item in move.Load)
            {
                double fromZ = item.Z;

                item.MoveTo(move.To.X, move.To.Y, Math.Round(fromZ + move.Lift, 4), item.Rotation);
                move.Slid.Add(new SlidingItem(item.Id, fromZ, item.Z));
                _rolledItemIds.Add(item.Id);
                _unsavedPositions.Add(item.Id);
            }

            if (move.Load.Count > 0)
            {
                sliding[move.From] = move.To;
            }
        }

        _slidingStacks = sliding;
        _slidesShownUntil = Environment.TickCount64 + SlideMilliseconds;

        if (sliding.Count > 0)
        {
            RebuildTileIndex();
        }
    }

    private RollerSlide? FinishRoll(RollerMove move)
    {
        SlidingRider? rider = move.Rider is { Actor: var actor } ? MoveRider(actor, move) : null;
        return move.Slid.Count == 0 && rider is null
            ? null
            : new RollerSlide(move.Roller.Id, move.From, move.To, move.Slid, rider);
    }

    private SlidingRider? MoveRider(RoomActor actor, RollerMove move)
    {
        lock (actor.Lock)
        {
            if (actor.X != move.From.X || actor.Y != move.From.Y || actor.IsWalking || actor.PendingStep is not null || actor.ActiveOneWayDoor is not null
                || (actor.ActiveTeleport is { } teleport && !move.Load.Any(item => item.Id == teleport.TeleporterItemId)))
            {
                return null;
            }

            double fromZ = actor.Z;
            actor.X = move.To.X;
            actor.Y = move.To.Y;
            TryReachBusTile(actor);

            if (move.Load.Count > 0)
            {
                actor.Z = Math.Round(fromZ + move.Lift, 4);
                return new SlidingRider(actor.VirtualId, fromZ, actor.Z, actor.Posture, actor.PostureHeight, actor.RotHead, actor.RotBody);
            }

            ApplyTilePosture(actor);
            if (actor.Posture != ActorPosture.Stand)
            {
                _ridersToSettle.Add(actor);
            }

            return new SlidingRider(actor.VirtualId, fromZ, actor.Z, ActorPosture.Stand, 0, actor.RotHead, actor.RotBody);
        }
    }

    private IReadOnlyList<RoomFloorItem> LoadOf(RoomFloorItem roller) =>
        ItemsAtUnlocked(roller.X, roller.Y).Where(item => item != roller && item.Z >= roller.SurfaceZ - HeightTolerance).ToList();

    private RoomFloorItem? TopRollerAt((int X, int Y) tile) =>
        ItemsAtUnlocked(tile.X, tile.Y).LastOrDefault(item => item.IsRoller) is { IsSingleTile: true } roller ? roller : null;

    private static Occupant? OccupantOn(List<Occupant> occupants, (int X, int Y) tile)
    {
        foreach (Occupant occupant in occupants)
        {
            if (occupant.Tile == tile)
            {
                return occupant;
            }
        }

        return null;
    }

    private readonly record struct Occupant(
        RoomActor Actor, (int X, int Y) Tile, double Z, bool IsMoving, (int X, int Y)? SteppingOnto, int? InTeleporter);

    private sealed class RollerMove(RoomFloorItem roller, IReadOnlyList<RoomFloorItem> load, Occupant? rider)
    {
        public RoomFloorItem Roller { get; } = roller;

        public (int X, int Y) From { get; } = (roller.X, roller.Y);

        public (int X, int Y) To { get; } = roller.SquareInFront();

        public IReadOnlyList<RoomFloorItem> Load { get; } = load;

        public Occupant? Rider { get; } = rider;

        public double Landing { get; set; }

        public double Lift => Landing - Roller.SurfaceZ;

        public List<SlidingItem> Slid { get; } = [];
    }
}
