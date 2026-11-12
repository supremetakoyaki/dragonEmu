namespace DragonEmu.Domain;

public sealed record SubscriptionGrantResult(
    DateTime ClubExpiresAt,
    int ClubMonthsAccumulated,
    bool IsVip = false);
