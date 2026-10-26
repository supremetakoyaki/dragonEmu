using System.Text.RegularExpressions;

namespace DragonEmu.Domain;

public static partial class CreditFurni
{
    public static int? ValueOf(string classname) =>
        Classname().Match(classname) is { Success: true } match && int.TryParse(match.Groups["credits"].ValueSpan, out int credits) && credits > 0
            ? credits
            : null;

    [GeneratedRegex(@"^CFC?_(?<credits>\d+)_")]
    private static partial Regex Classname();
}
