using DragonEmu.Domain;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.InventoryPurse;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Rewards;

public sealed class OnlineRewardService : IHostedService, IDisposable
{
    public static readonly TimeSpan Tick = TimeSpan.FromSeconds(30);

    private const int AccountsPerStatement = 500;

    private readonly ClientSessionManager _sessions;
    private readonly IRewardSettingsRepository _settingsRepository;
    private readonly ICurrencyRepository _currencies;
    private readonly ILogger<OnlineRewardService> _logger;
    private readonly PeriodicTimer _timer = new(Tick);
    private CancellationTokenSource? _stoppingCts;
    private Task? _loop;

    public OnlineRewardService(
        ClientSessionManager sessions, IRewardSettingsRepository settingsRepository, ICurrencyRepository currencies, ILogger<OnlineRewardService> logger)
    {
        _sessions = sessions;
        _settingsRepository = settingsRepository;
        _currencies = currencies;
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
                try
                {
                    await RewardAsync(await _settingsRepository.GetAsync(cancellationToken), cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Online rewards failed; the next round tries again.");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task RewardAsync(OnlineRewardSettings settings, CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        var creditsDue = new Dictionary<int, ClientSession>();
        var pixelsDue = new Dictionary<int, ClientSession>();
        foreach (ClientSession session in _sessions.GetAuthenticatedSessions())
        {
            if (session.AuthenticatedUser is not { } user)
            {
                continue;
            }

            if (Due(session.CreditReward, settings.CreditsOn, settings.CreditInterval, now, session.LoggedInAt))
            {
                creditsDue.TryAdd(user.Id, session);
            }

            if (Due(session.PixelReward, settings.PixelsOn, settings.PixelInterval, now, session.LoggedInAt))
            {
                pixelsDue.TryAdd(user.Id, session);
            }
        }

        await PayAsync(creditsDue, CurrencyType.Credits, settings.CreditAmount, balance => new CreditBalanceEvent(balance).Compose(), cancellationToken);
        await PayAsync(pixelsDue, CurrencyType.ActivityPoints, settings.PixelAmount,
            balance => new HabboActivityPointNotificationMessageEvent(balance, settings.PixelAmount).Compose(), cancellationToken);
    }

    private static bool Due(RewardTimer timer, bool on, int intervalMinutes, DateTime now, DateTime? loggedInAt)
    {
        if (!on)
        {
            timer.Restart(now);
            return false;
        }

        return timer.TryClaim(now, TimeSpan.FromMinutes(intervalMinutes), loggedInAt);
    }

    private async Task PayAsync(
        Dictionary<int, ClientSession> due, int currency, int amount, Func<long, Protocol.ServerPacket> notice, CancellationToken cancellationToken)
    {
        foreach (int[] batch in due.Keys.Chunk(AccountsPerStatement))
        {
            IReadOnlyDictionary<int, long> balances = await _currencies.AddBalanceToManyAsync(batch, currency, amount, cancellationToken);
            foreach ((int userId, long balance) in balances)
            {
                due[userId].EnqueueSend(notice(balance));
            }
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
        _stoppingCts?.Dispose();
    }
}
