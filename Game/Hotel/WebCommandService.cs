using DragonEmu.Game.Achievements;
using DragonEmu.Game.Chat;
using DragonEmu.Game.Moderation;
using DragonEmu.Game.Settings;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Hotel;

public sealed class WebCommandService : IHostedService, IDisposable
{
    private const int BatchSize = 100;
    private const string EmailAchievement = "ACH_EmailVerification";
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(5);

    private readonly IWebCommandRepository _commands;
    private readonly ClientSessionManager _sessions;
    private readonly AchievementService _achievements;
    private readonly IModerationRepository _moderation;
    private readonly GameSettingsCache _gameSettings;
    private readonly ProfanityFilterCache _profanity;
    private readonly ILogger<WebCommandService> _logger;
    private readonly PeriodicTimer _timer = new(CheckInterval);
    private CancellationTokenSource? _stoppingCts;
    private Task? _loop;

    public WebCommandService(
        IWebCommandRepository commands,
        ClientSessionManager sessions,
        AchievementService achievements,
        IModerationRepository moderation,
        GameSettingsCache gameSettings,
        ProfanityFilterCache profanity,
        ILogger<WebCommandService> logger)
    {
        _commands = commands;
        _sessions = sessions;
        _achievements = achievements;
        _moderation = moderation;
        _gameSettings = gameSettings;
        _profanity = profanity;
        _logger = logger;
    }

    public async Task CheckAsync(CancellationToken cancellationToken = default)
    {
        foreach (WebCommand command in await _commands.GetPendingAsync(BatchSize, cancellationToken))
        {
            try
            {
                await RunAsync(command, cancellationToken);
                await _commands.DeleteAsync(command.Id, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Web command {Id} ({Command}) failed; the next look tries again.", command.Id, command.Command);
            }
        }
    }

    private async Task RunAsync(WebCommand command, CancellationToken cancellationToken)
    {
        switch (command.Command)
        {
            case "disconnect":
                if (_sessions.TryGetByUserId(command.UserId, out ClientSession? session) && session is not null)
                {
                    _logger.LogInformation("Session {SessionId} logged out everywhere from the website. Disconnecting...", session.Id);
                    session.Close();
                }

                break;
            case "email_verified":
                await _achievements.SetAsync(command.UserId, EmailAchievement, 1, cancellationToken);
                break;
            case "ban":
                if (_sessions.TryGetByUserId(command.UserId, out ClientSession? banned) && banned is not null
                    && await _moderation.GetActiveBanAsync(command.UserId, cancellationToken) is { } ban)
                {
                    _logger.LogInformation("Session {SessionId} banned from the website. Disconnecting...", banned.Id);
                    ModerationService.ShowBanAndDisconnect(banned, ban.Reason);
                }

                break;
            case "word_filter":
                await _gameSettings.ReloadProfanityFilterAsync(cancellationToken);
                await _profanity.ReloadAsync(cancellationToken);
                break;
            default:
                _logger.LogWarning("Web command {Id}: unknown command {Command}, dropped.", command.Id, command.Command);
                break;
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
                    _logger.LogError(ex, "Looking for web commands failed; the next look tries again.");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose() => _timer.Dispose();
}
