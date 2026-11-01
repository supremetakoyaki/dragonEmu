using DragonEmu.Game.Settings;
using DragonEmu.Gateway;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Hotel;

public sealed class HotelViewIdleService : IHostedService, IDisposable
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);

    private readonly ClientSessionManager _sessions;
    private readonly GameSettingsCache _gameSettings;
    private readonly ILogger<HotelViewIdleService> _logger;
    private readonly PeriodicTimer _timer = new(CheckInterval);
    private CancellationTokenSource? _stoppingCts;
    private Task? _loop;

    public HotelViewIdleService(ClientSessionManager sessions, GameSettingsCache gameSettings, ILogger<HotelViewIdleService> logger)
    {
        _sessions = sessions;
        _gameSettings = gameSettings;
        _logger = logger;
    }

    public static bool IsDue(ClientSession session, int minutes, DateTime now) =>
        minutes > 0
        && session is { CurrentRoomId: null, LoadingRoomId: null, IsModerator: false }
        && now - session.LastActiveAt >= TimeSpan.FromMinutes(minutes);

    public void Check(DateTime now)
    {
        int minutes = _gameSettings.Current.IdleDisconnectTime;
        foreach (ClientSession session in _sessions.GetAuthenticatedSessions())
        {
            if (IsDue(session, minutes, now))
            {
                _logger.LogInformation("Session {SessionId} idle in the hotel view for {Minutes} minutes. Disconnecting...", session.Id, minutes);
                session.Close();
            }
        }
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
                try
                {
                    Check(DateTime.UtcNow);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Hotel view idle check failed; the next one tries again.");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose() => _timer.Dispose();
}
