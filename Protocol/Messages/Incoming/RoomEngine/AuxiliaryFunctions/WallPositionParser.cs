namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine.AuxiliaryFunctions;

internal static class WallPositionParser
{
    private const int MaxOffset = 200;

    public static bool TryParse(string raw, out int wallX, out int wallY, out int offsetX, out int offsetY, out string direction)
    {
        wallX = wallY = offsetX = offsetY = 0;
        direction = string.Empty;

        string[] parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 || (parts[2] != "l" && parts[2] != "r"))
        {
            return false;
        }

        if (!parts[0].StartsWith(":w=", StringComparison.Ordinal) || !parts[1].StartsWith("l=", StringComparison.Ordinal))
        {
            return false;
        }

        string[] wallCoords = parts[0][3..].Split(',');
        string[] offsetCoords = parts[1][2..].Split(',');
        if (wallCoords.Length != 2 || offsetCoords.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(wallCoords[0], out wallX) || !int.TryParse(wallCoords[1], out wallY)
            || !int.TryParse(offsetCoords[0], out offsetX) || !int.TryParse(offsetCoords[1], out offsetY))
        {
            return false;
        }

        if (offsetX < 0 || offsetX > MaxOffset || offsetY < 0 || offsetY > MaxOffset)
        {
            return false;
        }

        direction = parts[2];
        return true;
    }

    public static string Format(int wallX, int wallY, int offsetX, int offsetY, string direction) =>
        $":w={wallX},{wallY} l={offsetX},{offsetY} {direction}";
}
