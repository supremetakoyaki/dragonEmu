namespace DragonEmu.Gateway;

public sealed class OnlineTimeMeter
{
    private long _countedUpTo;

    public void Start(DateTime now) => Interlocked.Exchange(ref _countedUpTo, now.Ticks);

    public int Take(DateTime now)
    {
        long from = Interlocked.Read(ref _countedUpTo);
        while (from != 0)
        {
            long seconds = (now.Ticks - from) / TimeSpan.TicksPerSecond;
            if (seconds <= 0)
            {
                return 0;
            }

            long seen = Interlocked.CompareExchange(ref _countedUpTo, from + seconds * TimeSpan.TicksPerSecond, from);
            if (seen == from)
            {
                return (int)Math.Min(seconds, int.MaxValue);
            }

            from = seen;
        }

        return 0;
    }
}
