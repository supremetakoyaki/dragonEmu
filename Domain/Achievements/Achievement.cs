namespace DragonEmu.Domain;

public sealed record AchievementLevel(int Level, int Threshold, string BadgeCode, int RewardActivityPoints, int RewardAchievementScore = 0,
    int RewardActivityPointType = ActivityPointType.Pixels)
{
    public int RewardCurrency => CurrencyType.OfActivityPoints(RewardActivityPointType);
}

public sealed record AchievementDefinition(int Id, string CodeBase, int MaxLevel, bool Enabled, IReadOnlyList<AchievementLevel> Levels,
    int ClientType = 0);

public sealed record AchievementProgress(int AchievementId, int Counter, int CurrentLevel);
