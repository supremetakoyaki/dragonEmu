namespace DragonEmu.Domain;

public sealed record CatalogPageSummary(
    int Id,
    int ParentId,
    int Color,
    int Icon,
    string DisplayName,
    bool ComingSoon,
    bool Enabled,
    int MinRank);
