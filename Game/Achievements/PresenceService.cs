using DragonEmu.Gateway;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Achievements;

public sealed class PresenceService : IHostedService, IDisposable
{
    public const int TraderPassSeconds = 3600;

    private const int AccountsPerStatement = 500;
    private const string HotelPresence = "ACH_AllTimeHotelPresence";
    private const string TraderPass = "ACH_TraderPass";

    private static readonly TimeSpan CreditInterval = TimeSpan.FromMinutes(15);

    private readonly ClientSessionManager _sessionManager;
    private readonly IUserRepository _userRepository;
    private readonly AchievementService _achievementService;
    private readonly ILogger<PresenceService> _logger;
    private readonly PeriodicTimer _timer = new(CreditInterval);
    private CancellationTokenSource? _stoppingCts;
    private Task? _loop;

    public PresenceService(
        ClientSessionManager sessionManager, IUserRepository userRepository, AchievementService achievementService, ILogger<PresenceService> logger)
    {
        _sessionManager = sessionManager;
        _userRepository = userRepository;
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

    public async Task OnLoginAsync(int userId, CancellationToken cancellationToken)
    {
        if (await _userRepository.GetOnlineSecondsAsync(userId, cancellationToken) >= TraderPassSeconds)
        {
            await _achievementService.RaiseToAsync(userId, TraderPass, 1, cancellationToken);
        }
    }

    public async Task FlushAsync(ClientSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } user && session.OnlineTime.Take(DateTime.UtcNow) is var seconds and > 0)
        {
            await CreditAsync(new Dictionary<int, int> { [user.Id] = seconds }, cancellationToken);
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
                    await CreditEveryoneAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Crediting online time failed; the next round tries again.");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task CreditEveryoneAsync()
    {
        DateTime now = DateTime.UtcNow;
        var secondsByUser = new Dictionary<int, int>();
        foreach (ClientSession session in _sessionManager.GetAuthenticatedSessions())
        {
            if (session.AuthenticatedUser is { } user && session.OnlineTime.Take(now) is var seconds and > 0)
            {
                secondsByUser[user.Id] = secondsByUser.GetValueOrDefault(user.Id) + seconds;
            }
        }

        foreach (KeyValuePair<int, int>[] batch in secondsByUser.Chunk(AccountsPerStatement))
        {
            await CreditAsync(batch.ToDictionary(), CancellationToken.None);
        }
    }

    private async Task CreditAsync(IReadOnlyDictionary<int, int> secondsByUser, CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<int, int> totals = await _userRepository.AddOnlineSecondsAsync(secondsByUser, cancellationToken);
        foreach ((int userId, int total) in totals)
        {
            int before = total - secondsByUser[userId];
            if (total / 3600 == before / 3600)
            {
                continue;
            }

            await _achievementService.SetAsync(userId, HotelPresence, total / 3600, cancellationToken);
            if (before < TraderPassSeconds)
            {
                await _achievementService.RaiseToAsync(userId, TraderPass, 1, cancellationToken);
            }
        }
    }

    public void Dispose() => _timer.Dispose();
}
