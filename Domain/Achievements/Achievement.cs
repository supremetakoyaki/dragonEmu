namespace DragonEmu.Domain;

public sealed record AchievementLevel(int Level, int Threshold, string BadgeCode, int RewardActivityPoints);

public sealed record AchievementDefinition(int Id, string CodeBase, int MaxLevel, bool Enabled, IReadOnlyList<AchievementLevel> Levels);

public sealed record AchievementProgress(int AchievementId, int Counter, int CurrentLevel);
