namespace DragonEmu.Domain;

public sealed record FireworkDefinition(
    int FurniDefId,
    int PriceCredits,
    int PriceActivityPoints,
    int ActivityPointType,
    int ChargeAmount,
    TimeSpan LaunchDuration);
