using System.Collections.Concurrent;

namespace DragonEmu.Game.Vouchers;

public sealed class VoucherAttemptGuard
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan CooldownDuration = TimeSpan.FromMinutes(10);

    private readonly ConcurrentDictionary<int, State> _states = new();

    public bool IsInCooldown(int userId)
    {
        if (!_states.TryGetValue(userId, out State? state))
        {
            return false;
        }

        lock (state)
        {
            if (state.CooldownUntil is not { } until)
            {
                return false;
            }

            if (DateTime.UtcNow < until)
            {
                return true;
            }

            _states.TryRemove(new KeyValuePair<int, State>(userId, state));
            return false;
        }
    }

    public void RecordFailure(int userId)
    {
        State state = _states.GetOrAdd(userId, static _ => new State());
        lock (state)
        {
            state.FailedCount++;
            if (state.FailedCount >= MaxAttempts)
            {
                state.CooldownUntil = DateTime.UtcNow.Add(CooldownDuration);
                state.FailedCount = 0;
            }
        }
    }

    public void RecordSuccess(int userId)
    {
        if (!_states.TryGetValue(userId, out State? state))
        {
            return;
        }

        _states.TryRemove(new KeyValuePair<int, State>(userId, state));
    }

    private sealed class State
    {
        public int FailedCount;
        public DateTime? CooldownUntil;
    }
}
