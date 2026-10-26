namespace DragonEmu.Domain;

public sealed class ChatFloodGuard
{
    public const int MaxMessages = 8;
    public static readonly TimeSpan Window = TimeSpan.FromSeconds(3);
    public static readonly TimeSpan BlockFor = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan TellEvery = TimeSpan.FromSeconds(5);

    private readonly Queue<long> _recent = new(MaxMessages);
    private readonly object _gate = new();
    private long _blockedUntil;
    private long? _toldAt;

    public bool TryPass(long nowMs, out int secondsLeft, out bool tell)
    {
        lock (_gate)
        {
            if (nowMs >= _blockedUntil)
            {
                while (_recent.Count > 0 && nowMs - _recent.Peek() >= (long)Window.TotalMilliseconds)
                {
                    _recent.Dequeue();
                }

                if (_recent.Count < MaxMessages)
                {
                    _recent.Enqueue(nowMs);
                    secondsLeft = 0;
                    tell = false;
                    return true;
                }

                _recent.Clear();
                _blockedUntil = nowMs + (long)BlockFor.TotalMilliseconds;
                _toldAt = null;
            }

            secondsLeft = (int)Math.Ceiling((_blockedUntil - nowMs) / 1000.0);
            tell = _toldAt is not { } toldAt || nowMs - toldAt >= (long)TellEvery.TotalMilliseconds;
            if (tell)
            {
                _toldAt = nowMs;
            }

            return false;
        }
    }
}
