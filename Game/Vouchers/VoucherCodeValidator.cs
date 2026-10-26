using System.Text.RegularExpressions;

namespace DragonEmu.Game.Vouchers;

public static class VoucherCodeValidator
{
    private static readonly Regex Pattern = new("^[a-hj-km-np-vx-z]{6,7}$", RegexOptions.Compiled);

    public static bool IsValidFormat(string code) => Pattern.IsMatch(code);
}
