using DragonEmu.Game.Settings;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.RoomAction;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Rooms;

public sealed class RoomIdleService : IHostedService, IDisposable
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(5);

    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;
    private readonly RoomKickService _kicks;
    private readonly GameSettingsCache _gameSettings;
    private readonly ILogger<RoomIdleService> _logger;
    private readonly PeriodicTimer _timer = new(CheckInterval);
    private CancellationTokenSource? _stoppingCts;
    private Task? _loop;

    public RoomIdleService(
        IRoomManager roomManager, RoomBroadcaster broadcaster, RoomKickService kicks, GameSettingsCache gameSettings,
        ILogger<RoomIdleService> logger)
    {
        _roomManager = roomManager;
        _broadcaster = broadcaster;
        _kicks = kicks;
        _gameSettings = gameSettings;
        _logger = logger;
    }

    public void MarkActive(IGameSession session)
    {
        if (session.CurrentRoomId is { } roomId && _roomManager.TryGetLoadedRoom(roomId, out Room? room)
            && room!.TryGetActor(session.Id, out RoomActor? actor) && Room.WakeUp(actor!, DateTime.UtcNow))
        {
            _broadcaster.Broadcast(room, new SleepMessageEvent(actor!.VirtualId, asleep: false).Compose());
        }
    }

    public async Task CheckAsync(CancellationToken cancellationToken)
    {
        int sleepSeconds = _gameSettings.Current.RoomIdleTimeSleep;
        if (sleepSeconds <= 0)
        {
            return;
        }

        int kickSeconds = _gameSettings.Current.RoomIdleTimeKick;
        TimeSpan sleepAfter = TimeSpan.FromSeconds(sleepSeconds);
        TimeSpan? kickAfter = kickSeconds > 0 ? TimeSpan.FromSeconds(kickSeconds) : null;
        DateTime now = DateTime.UtcNow;

        foreach (Room room in _roomManager.LoadedRooms)
        {
            IdleCheckResult result = room.CheckIdle(now, sleepAfter, kickAfter);
            foreach (RoomActor sleeper in result.FellAsleep)
            {
                _broadcaster.Broadcast(room, new SleepMessageEvent(sleeper.VirtualId, asleep: true).Compose());
            }

            foreach (RoomActor sleeper in result.SleptTooLong)
            {
                await _kicks.PutOutIdleAsync(room, sleeper, cancellationToken);
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
                    await CheckAsync(cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Idle check failed; the next one tries again.");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose() => _timer.Dispose();
}
