using System.Security.Cryptography;

namespace DragonEmu.Game.Camera;

public sealed class CameraState
{
    private const string TokenCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

    private string? _token;
    private long _lastSaveTicks;

    public bool IsOut => Volatile.Read(ref _token) is not null;

    public string NewToken()
    {
        string token = RandomNumberGenerator.GetString(TokenCharacters, 16);
        Volatile.Write(ref _token, token);
        return token;
    }

    public string? TakeToken() => Interlocked.Exchange(ref _token, null);

    public void PutAway() => Volatile.Write(ref _token, null);

    public bool TrySave(DateTime now, TimeSpan interval)
    {
        long last = Volatile.Read(ref _lastSaveTicks);
        return now.Ticks - last >= interval.Ticks && Interlocked.CompareExchange(ref _lastSaveTicks, now.Ticks, last) == last;
    }
}
