namespace DragonEmu.Domain;

public static class ClothingGateOutfits
{
    private const char Separator = ',';

    public static string? For(string? extraData, char gender)
    {
        if (string.IsNullOrEmpty(extraData))
        {
            return null;
        }

        string[] parts = extraData.Split(Separator);
        int index = char.ToUpperInvariant(gender) == 'F' ? 1 : 0;
        return index < parts.Length && LooksLikeFigure(parts[index]) ? parts[index] : null;
    }

    public static string With(string? extraData, char gender, string figure)
    {
        string? boy = For(extraData, 'M');
        string? girl = For(extraData, 'F');
        if (char.ToUpperInvariant(gender) == 'F')
        {
            girl = figure;
        }
        else
        {
            boy = figure;
        }

        return string.Concat(boy, Separator, girl);
    }

    public static readonly IReadOnlyList<string> ClothingTypes = ["ch", "cc", "ca", "cp", "lg", "wa", "sh"];

    public static string Combine(string outfit, string ownFigure)
    {
        if (!Tokens(ownFigure).Any(IsHead))
        {
            return outfit;
        }

        return string.Join('.', Tokens(ownFigure).Where(token => !IsClothing(token)).Concat(Tokens(outfit).Where(IsClothing)));
    }

    private static bool IsClothing(string token) => ClothingTypes.Contains(token.Split('-')[0]);

    private static bool IsHead(string token) => token.StartsWith("hd-", StringComparison.Ordinal);

    private static IEnumerable<string> Tokens(string figure) => figure.Split('.', StringSplitOptions.RemoveEmptyEntries);

    private static bool LooksLikeFigure(string value) => value.Contains('-');
}
