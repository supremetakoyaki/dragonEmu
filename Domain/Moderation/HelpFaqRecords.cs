namespace DragonEmu.Domain;

public enum FaqFrontPage
{
    None,
    Urgent,
    Normal,
}

public sealed record FaqEntry(int Id, int CategoryId, string Question, string Answer, FaqFrontPage FrontPage);

public sealed record FaqCategory(int Id, string Name, string Description, IReadOnlyList<FaqEntry> Entries);
