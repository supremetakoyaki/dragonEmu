namespace DragonEmu.Domain;

public sealed record CatalogPageBody(
    int PageId,
    string LayoutCode,
    IReadOnlyList<string> LocalizationImages,
    IReadOnlyList<string> LocalizationTexts);
