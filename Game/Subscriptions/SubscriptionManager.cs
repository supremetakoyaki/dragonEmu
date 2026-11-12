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

    public async Task<bool> IsVipAsync(int userId, CancellationToken cancellationToken = default) =>
        await IsClubMemberAsync(userId, cancellationToken) && await _userRepository.IsVipAsync(userId, cancellationToken);

    public async Task<ClubMembership> GetMembershipAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (!await IsClubMemberAsync(userId, cancellationToken))
        {
            return ClubMembership.None;
        }

        return await _userRepository.IsVipAsync(userId, cancellationToken) ? ClubMembership.Vip : ClubMembership.HabboClub;
    }

    public async Task<(bool IsVip, ClubGiftRules.Standing Standing)> GetClubSummaryAsync(int userId, CancellationToken cancellationToken = default) =>
        (await IsVipAsync(userId, cancellationToken), await _userRepository.GetClubGiftStandingAsync(userId, cancellationToken));

    public async Task<SubscriptionGrantResult> GrantClubOfferAsync(int userId, ClubOffer offer, CancellationToken cancellationToken = default)
    {
        (DateTime? clubExpiresAt, int monthsAccumulated, _) = await _userRepository.GetClubStatusAsync(userId, cancellationToken);

        DateTime newExpiresAt = ClubOfferRules.EndAfter(clubExpiresAt, DateTime.UtcNow, offer.PeriodDays);
        int newMonthsAccumulated = monthsAccumulated + offer.Months;

        await _userRepository.UpdateClubStatusAsync(userId, newExpiresAt, newMonthsAccumulated, cancellationToken);
        await _userRepository.SetVipAsync(userId, offer.Vip, cancellationToken);
        if (offer.Vip)
        {
            await _userRepository.AddVipDaysAsync(userId, offer.PeriodDays, cancellationToken);
        }

        _logger.LogDebug(
            "User {UserId} bought {Offer} ({Days} days, VIP {Vip}), now expires {ExpiresAt}.", userId, offer.Offer.OfferCode, offer.PeriodDays, offer.Vip, newExpiresAt);

        return new SubscriptionGrantResult(newExpiresAt, newMonthsAccumulated, offer.Vip);
    }

    public async Task<SubscriptionGrantResult> GrantClubUpgradeAsync(int userId, int vipDays, CancellationToken cancellationToken = default)
    {
        (_, int monthsAccumulated, _) = await _userRepository.GetClubStatusAsync(userId, cancellationToken);

        DateTime newExpiresAt = DateTime.UtcNow.AddDays(vipDays);
        await _userRepository.UpdateClubStatusAsync(userId, newExpiresAt, monthsAccumulated, cancellationToken);
        await _userRepository.SetVipAsync(userId, true, cancellationToken);
        await _userRepository.AddVipDaysAsync(userId, vipDays, cancellationToken);

        _logger.LogDebug("User {UserId} upgraded Habbo Club to VIP: {Days} days, until {ExpiresAt}.", userId, vipDays, newExpiresAt);

        return new SubscriptionGrantResult(newExpiresAt, monthsAccumulated, IsVip: true);
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
