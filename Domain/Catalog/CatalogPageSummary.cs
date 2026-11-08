namespace DragonEmu.Domain;

public sealed record CatalogPageSummary(
    int Id,
    int ParentId,
    int Color,
    int Icon,
    LocalizedText DisplayName,
    bool ComingSoon,
    bool Enabled,
    int MinRank)
{
    public static bool IsComingSoon(IReadOnlyList<CatalogPageSummary> pages, int pageId)
    {
        var seen = new HashSet<int>();
        while (seen.Add(pageId) && pages.FirstOrDefault(p => p.Id == pageId) is { } page)
        {
            if (page.ComingSoon)
            {
                return true;
            }

            pageId = page.ParentId;
        }

        return false;
    }
}
