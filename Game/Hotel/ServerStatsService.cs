using DragonEmu.Game.Settings;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Hotel;

public sealed class ServerStatsService : IHostedService
{
    private readonly IServerStatsRepository _repository;
    private readonly ClientSessionManager _sessions;
    private readonly HotelStatus _status;
    private readonly TimeProvider _time;
    private readonly ILogger<ServerStatsService> _logger;
    private long _startedAt;
    private CancellationTokenSource? _stoppingCts;
    private Task? _loop;

    public ServerStatsService(
        IServerStatsRepository repository, ClientSessionManager sessions, HotelStatus status, ILogger<ServerStatsService> logger,
        TimeProvider? time = null)
    {
        _repository = repository;
        _sessions = sessions;
        _status = status;
        _logger = logger;
        _time = time ?? TimeProvider.System;
    }

    public static TimeSpan RefreshInterval(int usersOnline) => usersOnline switch
    {
        < 10 => TimeSpan.FromMinutes(1),
        < 50 => TimeSpan.FromMinutes(5),
        _ => TimeSpan.FromMinutes(15),
    };

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _startedAt = _time.GetTimestamp();
        _status.ShutdownBegan += OnShutdownBegan;
        int online = await SaveAsync(serverUp: !_status.IsShuttingDown, cancellationToken);
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loop = RunAsync(online, _stoppingCts.Token);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _status.ShutdownBegan -= OnShutdownBegan;
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

        await SaveAsync(serverUp: false, cancellationToken);
    }

    private async Task RunAsync(int online, CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                await Task.Delay(RefreshInterval(online), _time, cancellationToken);
                online = await SaveAsync(serverUp: !_status.IsShuttingDown, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void OnShutdownBegan() => _ = SaveAsync(serverUp: false, CancellationToken.None);

    private async Task<int> SaveAsync(bool serverUp, CancellationToken cancellationToken)
    {
        int online = serverUp ? _sessions.OnlineUserCount : 0;
        try
        {
            await _repository.SaveAsync(new ServerStats(serverUp, _time.GetElapsedTime(_startedAt), online), cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Couldn't write the server stats (global_server_stats).");
        }

        return online;
    }
}
