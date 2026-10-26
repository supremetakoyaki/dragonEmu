namespace DragonEmu.Game.Rooms;

public static class RoomTextSanitizer
{
    public static string Sanitize(string text, int maxLength)
    {
        string cleaned = new(text.Where(c => c is not ((char)0x01) and not ((char)0x02)).ToArray());
        return cleaned.Length > maxLength ? cleaned[..maxLength] : cleaned;
    }
}
