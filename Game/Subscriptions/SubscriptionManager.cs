using DragonEmu.Domain;
using DragonEmu.Persistence;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Subscriptions;

public sealed class SubscriptionManager
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SubscriptionManager> _logger;

    public SubscriptionManager(IUserRepository userRepository, ILogger<SubscriptionManager> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<bool> IsClubMemberAsync(int userId, CancellationToken cancellationToken = default)
    {
        (DateTime? clubExpiresAt, _, _) = await _userRepository.GetClubStatusAsync(userId, cancellationToken);
        return clubExpiresAt is { } expiresAt && expiresAt > DateTime.UtcNow;
    }

    public async Task<SubscriptionGrantResult> GrantMonthsAsync(
        int userId, int months, CancellationToken cancellationToken = default)
    {
        (DateTime? clubExpiresAt, int monthsAccumulated, _) = await _userRepository.GetClubStatusAsync(userId, cancellationToken);

        DateTime now = DateTime.UtcNow;
        DateTime baseline = clubExpiresAt is { } existing && existing > now ? existing : now;
        DateTime newExpiresAt = baseline.AddMonths(months);
        int newMonthsAccumulated = monthsAccumulated + months;

        await _userRepository.UpdateClubStatusAsync(userId, newExpiresAt, newMonthsAccumulated, cancellationToken);

        _logger.LogDebug(
            "User {UserId} granted {Months} month(s) of Habbo Club, now expires {ExpiresAt} ({TotalMonths} total).",
            userId, months, newExpiresAt, newMonthsAccumulated);

        return new SubscriptionGrantResult(newExpiresAt, newMonthsAccumulated);
    }

    public async Task<SubscriptionGrantResult> GrantDaysAsync(
        int userId, double days, CancellationToken cancellationToken = default)
    {
        (DateTime? clubExpiresAt, int monthsAccumulated, _) = await _userRepository.GetClubStatusAsync(userId, cancellationToken);

        DateTime now = DateTime.UtcNow;
        DateTime baseline = clubExpiresAt is { } existing && existing > now ? existing : now;
        DateTime newExpiresAt = baseline.AddDays(days);

        await _userRepository.UpdateClubStatusAsync(userId, newExpiresAt, monthsAccumulated, cancellationToken);

        _logger.LogDebug(
            "User {UserId} granted {Days} day(s) of Habbo Club, now expires {ExpiresAt}.", userId, days, newExpiresAt);

        return new SubscriptionGrantResult(newExpiresAt, monthsAccumulated);
    }
}
