using DragonEmu.Game.Settings;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Hotel;

public sealed class ServerStatsService : IHostedLifecycleService
{
    private readonly IServerStatsRepository _repository;
    private readonly ClientSessionManager _sessions;
    private readonly HotelStatus _status;
    private readonly TimeProvider _time;
    private readonly ILogger<ServerStatsService> _logger;
    private long _startedAt;
    private CancellationTokenSource? _stoppingCts;
    private Task? _loop;
    private int _stopped;

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
        await ClearOnlineAsync(cancellationToken);
        int online = await SaveAsync(serverUp: !_status.IsShuttingDown, cancellationToken);
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loop = RunAsync(online, _stoppingCts.Token);
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => StopOnceAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => StopOnceAsync(cancellationToken);

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task StopOnceAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _stopped, 1) == 1)
        {
            return;
        }

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
        await ClearOnlineAsync(cancellationToken);
    }

    private async Task ClearOnlineAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _repository.ClearOnlineAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Couldn't mark everyone offline (users.online).");
        }
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
