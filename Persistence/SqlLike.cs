namespace DragonEmu.Persistence;

internal static class SqlLike
{
    public static string Escape(string text) =>
        text.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
}
