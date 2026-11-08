namespace DragonEmu.Domain;

public sealed record CatalogPageBody(
    int PageId,
    string LayoutCode,
    LocalizedList LocalizationImages,
    LocalizedList LocalizationTexts);
