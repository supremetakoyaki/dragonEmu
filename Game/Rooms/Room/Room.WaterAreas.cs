using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    private readonly object _effectLock = new();
    private readonly List<RoomActor> _effectChanges = [];

    private volatile bool _waterRecheck;

    public bool WaterRecheckPending => _waterRecheck;

    public int WaterEffectAt(int x, int y)
    {
        lock (_furnitureLock)
        {
            return _topItemByTile.TryGetValue((x, y), out RoomFloorItem? top) ? top.WaterEffectId : 0;
        }
    }

    private bool UpdateWaterEffect(RoomActor actor)
    {
        if (actor.Type != ActorType.Player)
        {
            return false;
        }

        int water = WaterEffectAt(actor.X, actor.Y);
        if (water == actor.WaterEffectId)
        {
            return false;
        }

        if (actor.WaterEffectId == 0)
        {
            actor.EffectBeforeWater = actor.EffectId;
        }

        actor.WaterEffectId = water;
        actor.EffectId = water > 0 ? water : actor.EffectBeforeWater;
        QueueEffectChange(actor);
        return true;
    }

    private void QueueEffectChange(RoomActor actor)
    {
        lock (_effectLock)
        {
            if (!_effectChanges.Contains(actor))
            {
                _effectChanges.Add(actor);
            }
        }
    }

    public bool TrySetEffect(RoomActor actor, int effectId, bool remember)
    {
        lock (actor.Lock)
        {
            if (actor.WaterEffectId > 0)
            {
                if (remember)
                {
                    actor.EffectBeforeWater = effectId;
                }

                return false;
            }

            if (actor.Team != GameTeam.None)
            {
                return false;
            }

            actor.EffectId = effectId;
            return true;
        }
    }

    public IReadOnlyList<RoomActor> AdvanceWaterAreas()
    {
        if (_waterRecheck)
        {
            _waterRecheck = false;
            foreach (RoomActor actor in _actors.Values)
            {
                lock (actor.Lock)
                {
                    if (!actor.IsWalking && actor.PendingStep is null)
                    {
                        UpdateWaterEffect(actor);
                    }
                }
            }
        }

        lock (_effectLock)
        {
            if (_effectChanges.Count == 0)
            {
                return [];
            }

            RoomActor[] changed = [.. _effectChanges];
            _effectChanges.Clear();
            return changed;
        }
    }
}
