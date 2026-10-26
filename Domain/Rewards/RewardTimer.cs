namespace DragonEmu.Domain;

public sealed class RewardTimer
{
    private DateTime? _countingFrom;

    public bool TryClaim(DateTime now, TimeSpan interval, DateTime? startedAt = null)
    {
        _countingFrom ??= startedAt ?? now;
        DateTime from = _countingFrom.Value;

        if (now - from < interval)
        {
            return false;
        }

        DateTime next = from + interval;
        _countingFrom = now - next >= interval ? now : next;
        return true;
    }

    public void Restart(DateTime now) => _countingFrom = now;
}
