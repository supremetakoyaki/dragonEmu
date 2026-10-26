using System.Globalization;
using DragonEmu.Domain;

namespace DragonEmu.Game.Avatars;

public static class FigureSanitizer
{
    public static string? Sanitize(FigureDataCache cache, string figure, char gender, bool isClubMember)
    {
        string[] tokens = figure.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            return null;
        }

        var repaired = new List<string>(tokens.Length);
        var seenMandatory = new HashSet<string>();

        foreach (string token in tokens)
        {
            string? rebuilt = SanitizeToken(cache, token, gender, isClubMember, seenMandatory);
            if (rebuilt is null)
            {
                return null;
            }

            if (rebuilt.Length > 0)
            {
                repaired.Add(rebuilt);
            }
        }

        foreach (string mandatoryType in cache.MandatorySettypeTypes)
        {
            if (!seenMandatory.Contains(mandatoryType))
            {
                return null;
            }
        }

        return string.Join('.', repaired);
    }

    private static string? SanitizeToken(FigureDataCache cache, string token, char gender, bool isClubMember, HashSet<string> seenMandatory)
    {
        string[] parts = token.Split('-');
        if (parts.Length < 2 || parts[0].Length != 2)
        {
            return null;
        }

        string type = parts[0];
        if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int setId) || setId <= 0)
        {
            return null;
        }

        FigureSettype? settype = cache.GetSettype(type);
        FigureSet? set = cache.GetSet(setId);
        if (settype is null || set is null || set.SettypeRef != type)
        {
            return null;
        }

        if (settype.Mandatory)
        {
            seenMandatory.Add(type);
        }

        if (set.Club && !isClubMember)
        {
            if (!settype.Mandatory)
            {
                return string.Empty;
            }

            FigureSet? fallback = cache.GetDefaultFreeSet(type, gender);
            if (fallback is null)
            {
                return null;
            }

            set = fallback;
            setId = fallback.Id;
        }

        var colorIds = new List<int>();
        if (set.Colorable)
        {
            for (int i = 2; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out int colorId))
                {
                    return null;
                }

                FigureColor? color = cache.GetColor(colorId);
                if (color is null || color.PaletteId != settype.PaletteId)
                {
                    return null;
                }

                if (color.Club && !isClubMember)
                {
                    FigureColor? fallbackColor = cache.GetDefaultFreeColor(settype.PaletteId);
                    if (fallbackColor is null)
                    {
                        return null;
                    }

                    colorIds.Add(fallbackColor.Id);
                }
                else
                {
                    colorIds.Add(colorId);
                }
            }
        }

        return colorIds.Count == 0
            ? string.Create(CultureInfo.InvariantCulture, $"{type}-{setId}")
            : string.Create(CultureInfo.InvariantCulture, $"{type}-{setId}-{string.Join('-', colorIds)}");
    }
}
