namespace DragonEmu.Domain;

public sealed record UserBadge(string BadgeCode, int? Slot, DateTime AwardedAt);
