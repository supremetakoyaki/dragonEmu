namespace DragonEmu.Domain;

public sealed class RepeatFilter
{
    public static readonly TimeSpan Window = TimeSpan.FromMilliseconds(50);

    private int _lastId = -1;
    private int _lastLength;
    private int _lastHash;
    private long _lastAt;

    public bool IsRepeat(int id, ReadOnlySpan<byte> body, long nowMs)
    {
        var hash = new HashCode();
        hash.AddBytes(body);
        int bodyHash = hash.ToHashCode();

        bool repeat = id == _lastId && body.Length == _lastLength && bodyHash == _lastHash
            && nowMs - _lastAt < (long)Window.TotalMilliseconds;

        _lastId = id;
        _lastLength = body.Length;
        _lastHash = bodyHash;
        _lastAt = nowMs;
        return repeat;
    }
}
