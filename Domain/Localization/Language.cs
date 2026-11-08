namespace DragonEmu.Domain;

public enum Language
{
    En,
    Es,
    PtBr,
}

public static class LanguageExtensions
{
    public static readonly IReadOnlyList<Language> FallbackOrder = [Language.En, Language.Es, Language.PtBr];

    public static string ToCode(this Language language) => language switch
    {
        Language.En => "en",
        Language.Es => "es",
        Language.PtBr => "pt-br",
        _ => throw new ArgumentOutOfRangeException(nameof(language), language, "Unknown language."),
    };

    public static Language? FromCode(string? code) => code?.Trim().ToLowerInvariant() switch
    {
        "en" => Language.En,
        "es" => Language.Es,
        "pt-br" => Language.PtBr,
        _ => null,
    };
}
