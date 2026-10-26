namespace DragonEmu.Game.Settings;

public sealed class HotelStatus
{
    private readonly object _lock = new();
    private volatile bool _shuttingDown;
    private volatile int _reopensAtMinuteOfDay;

    public bool IsShuttingDown => _shuttingDown;

    public (int Hour, int Minute) ReopensAt => (_reopensAtMinuteOfDay / 60, _reopensAtMinuteOfDay % 60);

    public bool TryBegin(TimeOnly reopensAt)
    {
        lock (_lock)
        {
            if (_shuttingDown)
            {
                return false;
            }

            _reopensAtMinuteOfDay = reopensAt.Hour * 60 + reopensAt.Minute;
            _shuttingDown = true;
            return true;
        }
    }
}
