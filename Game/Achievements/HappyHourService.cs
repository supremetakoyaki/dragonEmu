using DragonEmu.Game.Settings;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Achievements;

public sealed class HappyHourService : IHostedService, IDisposable
{
    private const string CodeBase = "ACH_HappyHour";
    private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(1);

    private readonly GameSettingsCache _gameSettings;
    private readonly HotelClock _clock;
    private readonly ClientSessionManager _sessionManager;
    private readonly AchievementService _achievementService;
    private readonly ILogger<HappyHourService> _logger;
    private readonly PeriodicTimer _timer = new(TickInterval);
    private CancellationTokenSource? _stoppingCts;
    private Task? _loop;

    public HappyHourService(
        GameSettingsCache gameSettings, HotelClock clock, ClientSessionManager sessionManager,
        AchievementService achievementService, ILogger<HappyHourService> logger)
    {
        _gameSettings = gameSettings;
        _clock = clock;
        _sessionManager = sessionManager;
        _achievementService = achievementService;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loop = RunAsync(_stoppingCts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _stoppingCts?.Cancel();

        if (_loop is not null)
        {
            try
            {
                await _loop.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(cancellationToken))
            {
                await SweepAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HappyHourService tick loop crashed.");
        }
    }

    private async Task SweepAsync(CancellationToken cancellationToken)
    {
        if (!IsHappyHourNow())
        {
            return;
        }

        foreach (int userId in _sessionManager.GetOnlineUserIds())
        {
            await _achievementService.SetAsync(userId, CodeBase, 1, cancellationToken);
        }
    }

    public bool IsHappyHourNow()
    {
        (_, TimeOnly weekdayStart, TimeOnly weekdayEnd, TimeOnly weekendStart, TimeOnly weekendEnd) = _gameSettings.Current.HappyHour;

        if (!_clock.TryGetNow(out DateTime localNow))
        {
            return false;
        }

        bool isWeekend = localNow.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        (TimeOnly start, TimeOnly end) = isWeekend ? (weekendStart, weekendEnd) : (weekdayStart, weekdayEnd);

        TimeOnly localTime = TimeOnly.FromDateTime(localNow);
        return localTime >= start && localTime < end;
    }

    public void Dispose() => _timer.Dispose();
}
