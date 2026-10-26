namespace DragonEmu.Domain;

public static class Stickie
{
    public const int NotesPerPad = 20;

    public const int MaxTextLength = 500;

    public const string DefaultColour = "FFFF33";

    public static readonly IReadOnlyList<string> Colours = ["9CCEFF", "FF9CFF", "9CFF9C", DefaultColour];

    public static bool IsHeart(string classname) => classname is "post.it.vd" or "post_it_vd";

    public static int NotesIn(string? padExtraData) =>
        int.TryParse(padExtraData, out int notes) && notes > 0 ? notes : 1;

    public static string ColourOf(string? noteExtraData)
    {
        string data = noteExtraData ?? string.Empty;
        int space = data.IndexOf(' ');
        return space < 0 ? data : data[..space];
    }

    public static string TextOf(string? noteExtraData)
    {
        string data = noteExtraData ?? string.Empty;
        int space = data.IndexOf(' ');
        return space < 0 ? string.Empty : data[(space + 1)..];
    }

    public static string? ValidColour(string classname, string colour)
    {
        string upper = colour.ToUpperInvariant();
        return IsHeart(classname)
            ? (upper == DefaultColour ? upper : null)
            : Colours.Contains(upper) ? upper : null;
    }

    public static string NoteData(string colour, string text) => $"{colour} {text}";
}
