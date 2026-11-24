using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public sealed record IceTagTickResult(IReadOnlyList<long> StartedSkating, IReadOnlyList<int> Caught, IReadOnlyList<(int AccountId, int Minutes)> MinutesSkated)
{
    public static IceTagTickResult None { get; } = new([], [], []);

    public bool IsEmpty => StartedSkating.Count == 0 && Caught.Count == 0 && MinutesSkated.Count == 0;
}

public sealed partial class Room
{
    private readonly object _iceLock = new();
    private readonly HashSet<long> _skaters = [];
    private readonly List<long> _iceStarted = [];
    private readonly List<int> _iceCaught = [];
    private readonly List<(int AccountId, int Minutes)> _iceMinutes = [];
    private long? _iceTagged;
    private long _iceTagFreeAtMs;

    private volatile bool _hasIcePatch;
    private volatile bool _iceTagOn;
    private volatile bool _iceRecheck;

    public bool IceTagBusy
    {
        get
        {
            lock (_iceLock)
            {
                return (_iceRecheck && (_hasIcePatch || _skaters.Count > 0))
                    || (_iceTagOn && _iceTagged is null && _skaters.Count > 0)
                    || _iceStarted.Count > 0 || _iceCaught.Count > 0 || _iceMinutes.Count > 0;
            }
        }
    }

    private void NoteIceItems()
    {
        _hasIcePatch = _floorItems.Values.Any(item => item.IsIcePatch);
        _iceTagOn = _floorItems.Values.Any(item => item.IsIceTagPole);
        _iceRecheck = true;
    }

    private bool IsIceUnlocked(int x, int y, double z) =>
        TopUnderBallsUnlocked(x, y) is { IsIcePatch: true } patch && IceTagRules.OnIce(z, patch.SurfaceZ);

    private void UpdateIce(RoomActor actor, long nowMs)
    {
        if (actor.Type != ActorType.Player || (!_hasIcePatch && !actor.OnIce))
        {
            return;
        }

        bool onIce;
        lock (_furnitureLock)
        {
            onIce = actor.Team == GameTeam.None && actor.WaterEffectId == 0 && IsIceUnlocked(actor.X, actor.Y, actor.Z);
        }

        if (onIce == actor.OnIce)
        {
            return;
        }

        lock (_iceLock)
        {
            if (onIce)
            {
                actor.OnIce = true;
                actor.IceSinceMs = nowMs - actor.IceCarryMs;
                _skaters.Add(actor.SessionId);
                _iceStarted.Add(actor.SessionId);

                bool tagged = _iceTagOn && _iceTagged is null;
                if (tagged)
                {
                    _iceTagged = actor.SessionId;
                    _iceTagFreeAtMs = nowMs + IceTagRules.TagCooldownMs;
                }

                actor.EffectId = IceTagRules.EffectOf(actor.Gender, tagged);
            }
            else
            {
                LeaveIceUnlocked(actor, nowMs);
                actor.EffectId = 0;
            }

            QueueEffectChange(actor);
        }
    }

    private void LeaveIceUnlocked(RoomActor actor, long nowMs)
    {
        actor.OnIce = false;
        _skaters.Remove(actor.SessionId);
        CountIceMinutesUnlocked(actor, nowMs);
        actor.IceCarryMs = Math.Max(0, nowMs - actor.IceSinceMs);
        if (_iceTagged == actor.SessionId)
        {
            _iceTagged = null;
        }
    }

    private void CountIceMinutesUnlocked(RoomActor actor, long nowMs)
    {
        long minutes = (nowMs - actor.IceSinceMs) / IceTagRules.MinuteMs;
        if (minutes > 0)
        {
            actor.IceSinceMs += minutes * IceTagRules.MinuteMs;
            _iceMinutes.Add((actor.AccountId, (int)minutes));
        }
    }

    private void LeaveIce(RoomActor actor)
    {
        lock (actor.Lock)
        {
            if (!actor.OnIce)
            {
                return;
            }

            lock (_iceLock)
            {
                LeaveIceUnlocked(actor, Environment.TickCount64);
            }
        }
    }

    public IceTagTickResult AdvanceIceTag(long nowMs)
    {
        if (!_hasIcePatch && !IceTagBusy)
        {
            lock (_iceLock)
            {
                if (_skaters.Count == 0)
                {
                    return IceTagTickResult.None;
                }
            }
        }

        lock (_movementLock)
        {
            if (_iceRecheck)
            {
                _iceRecheck = false;
                foreach (RoomActor actor in _actors.Values)
                {
                    lock (actor.Lock)
                    {
                        if (!actor.IsWalking && actor.PendingStep is null)
                        {
                            UpdateIce(actor, nowMs);
                        }
                    }
                }
            }

            long[] skaters;
            long? tagged;
            bool tagOn = _iceTagOn;
            lock (_iceLock)
            {
                skaters = [.. _skaters];
                tagged = _iceTagged;
            }

            if (!tagOn && tagged is { } untagged)
            {
                JustSkate(untagged, endsTag: true);
            }
            else if (tagOn && tagged is null && skaters.Length > 0)
            {
                MakeIt(skaters[Random.Shared.Next(skaters.Length)], tagger: null, nowMs);
            }
            else if (tagOn && tagged is { } it && nowMs >= _iceTagFreeAtMs && _actors.TryGetValue(it, out RoomActor? tagger))
            {
                (int x, int y) = PositionOf(tagger);
                var caught = new List<long>();
                foreach (long skater in skaters)
                {
                    if (skater != it && _actors.TryGetValue(skater, out RoomActor? other) && PositionOf(other) is var (ox, oy) && IceTagRules.CanTag(x, y, ox, oy))
                    {
                        caught.Add(skater);
                    }
                }

                if (caught.Count > 0 && MakeIt(caught[Random.Shared.Next(caught.Count)], tagger: it, nowMs))
                {
                    JustSkate(it, endsTag: false);
                }
            }

            foreach (long skater in skaters)
            {
                if (_actors.TryGetValue(skater, out RoomActor? actor))
                {
                    lock (actor.Lock)
                    {
                        if (actor.OnIce)
                        {
                            lock (_iceLock)
                            {
                                CountIceMinutesUnlocked(actor, nowMs);
                            }
                        }
                    }
                }
            }

            lock (_iceLock)
            {
                if (_iceStarted.Count == 0 && _iceCaught.Count == 0 && _iceMinutes.Count == 0)
                {
                    return IceTagTickResult.None;
                }

                var result = new IceTagTickResult([.. _iceStarted], [.. _iceCaught], [.. _iceMinutes]);
                _iceStarted.Clear();
                _iceCaught.Clear();
                _iceMinutes.Clear();
                return result;
            }
        }
    }

    private bool MakeIt(long sessionId, long? tagger, long nowMs)
    {
        if (!_actors.TryGetValue(sessionId, out RoomActor? actor))
        {
            return false;
        }

        lock (actor.Lock)
        lock (_iceLock)
        {
            if (!actor.OnIce || _iceTagged != tagger)
            {
                return false;
            }

            _iceTagged = sessionId;
            _iceTagFreeAtMs = nowMs + IceTagRules.TagCooldownMs;
            if (tagger is not null)
            {
                _iceCaught.Add(actor.AccountId);
            }

            actor.EffectId = IceTagRules.EffectOf(actor.Gender, tagged: true);
            QueueEffectChange(actor);
            return true;
        }
    }

    private void JustSkate(long sessionId, bool endsTag)
    {
        if (!_actors.TryGetValue(sessionId, out RoomActor? actor))
        {
            return;
        }

        lock (actor.Lock)
        lock (_iceLock)
        {
            if (endsTag && _iceTagged == sessionId)
            {
                _iceTagged = null;
            }

            if (actor.OnIce)
            {
                actor.EffectId = IceTagRules.EffectOf(actor.Gender, tagged: false);
                QueueEffectChange(actor);
            }
        }
    }

    public bool InEffectGame(RoomActor actor)
    {
        lock (actor.Lock)
        {
            return actor.OnIce || actor.Team != GameTeam.None;
        }
    }
}
