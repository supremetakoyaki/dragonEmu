using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    public const int BanzaiTeleportStayTicks = 8;

    public const string BanzaiTeleportStill = "0";
    public const string BanzaiTeleportSpinning = "1";
    public const string BanzaiTeleportSpinOnce = "100";

    private readonly object _banzaiTeleportLock = new();
    private readonly Dictionary<long, BanzaiTrip> _banzaiTrips = [];
    private readonly List<FurnitureStateChange> _banzaiTeleportStates = [];
    private int _banzaiTeleportTick;

    private volatile bool _hasBanzaiTeleports;

    public bool BanzaiTeleportsBusy
    {
        get
        {
            lock (_banzaiTeleportLock)
            {
                return _banzaiTrips.Count > 0 || _banzaiTeleportStates.Count > 0 || _banzaiStillAfterSpin.Count > 0;
            }
        }
    }

    private void NoteBanzaiTeleports()
    {
        int count = 0;
        foreach (RoomFloorItem item in _floorItems.Values)
        {
            if (item.IsBanzaiTeleport && ++count >= 2)
            {
                break;
            }
        }

        _hasBanzaiTeleports = count >= 2;
    }

    private void TryStartBanzaiTeleport(RoomActor actor)
    {
        if (!_hasBanzaiTeleports || actor.Type != ActorType.Player)
        {
            return;
        }

        RoomFloorItem? teleport;
        lock (_furnitureLock)
        {
            teleport = TopUnderBallsUnlocked(actor.X, actor.Y);
        }

        if (teleport is not { IsBanzaiTeleport: true })
        {
            return;
        }

        lock (_banzaiTeleportLock)
        {
            if (_banzaiTrips.TryGetValue(actor.SessionId, out BanzaiTrip? trip) && trip.Departing)
            {
                return;
            }

            DepartUnlocked(actor, teleport.Id, _banzaiTeleportTick + 2);
        }
    }

    private void DepartUnlocked(RoomActor actor, int teleportId, int dueTick)
    {
        actor.IsWalking = false;
        actor.Path = null;
        actor.BanzaiTeleportHeld = true;
        _banzaiTrips[actor.SessionId] = new BanzaiTrip(teleportId, Departing: true, Tick: dueTick);
        _banzaiTeleportStates.Add(new FurnitureStateChange(teleportId, BanzaiTeleportSpinning));
    }

    public IReadOnlyList<FurnitureStateChange> AdvanceBanzaiTeleports(List<RoomActor> changed, out bool busy)
    {
        lock (_movementLock)
        lock (_banzaiTeleportLock)
        {
            _banzaiTeleportTick++;
            if (_banzaiTrips.Count == 0 && _banzaiTeleportStates.Count == 0 && _banzaiStillAfterSpin.Count == 0)
            {
                busy = false;
                return [];
            }

            SettleSpunTeleportsUnlocked();

            foreach ((long sessionId, BanzaiTrip trip) in _banzaiTrips.ToList())
            {
                if (!_actors.TryGetValue(sessionId, out RoomActor? actor))
                {
                    _banzaiTrips.Remove(sessionId);
                    if (trip.Departing)
                    {
                        _banzaiTeleportStates.Add(new FurnitureStateChange(trip.TeleportId, BanzaiTeleportStill));
                    }

                    continue;
                }

                lock (actor.Lock)
                {
                    if (trip.Departing)
                    {
                        if (_banzaiTeleportTick >= trip.Tick)
                        {
                            ArriveUnlocked(actor, trip, changed);
                        }

                        continue;
                    }

                    bool stillThere;
                    lock (_furnitureLock)
                    {
                        stillThere = !actor.IsWalking && actor.PendingStep is null
                            && TopUnderBallsUnlocked(actor.X, actor.Y) is { IsBanzaiTeleport: true } under && under.Id == trip.TeleportId;
                    }

                    if (!stillThere)
                    {
                        _banzaiTrips.Remove(sessionId);
                    }
                    else if (_banzaiTeleportTick - trip.Tick >= BanzaiTeleportStayTicks - 1)
                    {
                        DepartUnlocked(actor, trip.TeleportId, _banzaiTeleportTick + 1);
                    }
                }
            }

            FurnitureStateChange[] states = [.. _banzaiTeleportStates];
            _banzaiTeleportStates.Clear();
            busy = _banzaiTrips.Count > 0 || _banzaiStillAfterSpin.Count > 0;
            return states;
        }
    }

    private void ArriveUnlocked(RoomActor actor, BanzaiTrip trip, List<RoomActor> changed)
    {
        actor.BanzaiTeleportHeld = false;
        _banzaiTeleportStates.Add(new FurnitureStateChange(trip.TeleportId, BanzaiTeleportStill));

        var taken = new HashSet<(int X, int Y)>();
        foreach (RoomActor other in _actors.Values)
        {
            if (other.SessionId == actor.SessionId)
            {
                continue;
            }

            lock (other.Lock)
            {
                taken.Add((other.X, other.Y));
                if (other.PendingStep is { } step)
                {
                    taken.Add((step.X, step.Y));
                }
            }
        }

        List<RoomFloorItem> free;
        lock (_furnitureLock)
        {
            free = _floorItems.Values
                .Where(item => item.IsBanzaiTeleport && item.Id != trip.TeleportId && !taken.Contains((item.X, item.Y))
                    && TopUnderBallsUnlocked(item.X, item.Y)?.Id == item.Id)
                .ToList();
        }

        if (free.Count == 0)
        {
            _banzaiTrips.Remove(actor.SessionId);
            return;
        }

        RoomFloorItem to = free[Random.Shared.Next(free.Count)];
        actor.X = to.X;
        actor.Y = to.Y;
        actor.Z = SurfaceHeightAt(to.X, to.Y);
        actor.PendingStep = null;
        actor.IsWalking = false;
        actor.Path = null;
        actor.Goal = null;
        ApplyTilePosture(actor);
        UpdateWaterEffect(actor);
        if (!changed.Contains(actor))
        {
            changed.Add(actor);
        }

        _banzaiTeleportStates.Add(new FurnitureStateChange(to.Id, BanzaiTeleportSpinOnce));
        _banzaiTrips[actor.SessionId] = new BanzaiTrip(to.Id, Departing: false, Tick: _banzaiTeleportTick);
        _banzaiStillAfterSpin.Add((to.Id, _banzaiTeleportTick + 1));
    }

    private readonly List<(int ItemId, int Tick)> _banzaiStillAfterSpin = [];

    private void SettleSpunTeleportsUnlocked()
    {
        _banzaiStillAfterSpin.RemoveAll(spun =>
        {
            if (spun.Tick > _banzaiTeleportTick)
            {
                return false;
            }

            _banzaiTeleportStates.Add(new FurnitureStateChange(spun.ItemId, BanzaiTeleportStill));
            return true;
        });
    }

    private sealed record BanzaiTrip(int TeleportId, bool Departing, int Tick);
}
