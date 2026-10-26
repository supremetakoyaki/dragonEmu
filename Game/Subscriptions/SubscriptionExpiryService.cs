using DragonEmu.Game.Fuses;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Subscriptions;

public sealed class SubscriptionExpiryService : IHostedService, IDisposable
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(1);

    private readonly IUserRepository _userRepository;
    private readonly FuseService _fuses;
    private readonly ClientSessionManager _sessionManager;
    private readonly ILogger<SubscriptionExpiryService> _logger;
    private readonly PeriodicTimer _timer = new(TickInterval);
    private CancellationTokenSource? _stoppingCts;
    private Task? _loop;
    private DateTime _lookedUpTo;

    public SubscriptionExpiryService(
        IUserRepository userRepository, FuseService fuses, ClientSessionManager sessionManager, ILogger<SubscriptionExpiryService> logger)
    {
        _userRepository = userRepository;
        _fuses = fuses;
        _sessionManager = sessionManager;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lookedUpTo = DateTime.UtcNow;
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
            _logger.LogError(ex, "SubscriptionExpiryService tick loop crashed.");
        }
    }

    private async Task SweepAsync(CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        IReadOnlyList<int> ended = await _userRepository.GetUserIdsWhoseClubEndedAsync(_lookedUpTo, now, cancellationToken);
        _lookedUpTo = now;

        foreach (int userId in ended)
        {
            _logger.LogDebug("User {UserId}'s Habbo Club period ended.", userId);
            if (_sessionManager.TryGetByUserId(userId, out ClientSession? session) && session!.AuthenticatedUser is { } account)
            {
                await _fuses.SendHeldAsync(session, account, cancellationToken);
            }
        }
    }

    public void Dispose() => _timer.Dispose();
}
