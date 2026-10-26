using System.Text;

namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    private const int MaxSkipRun = 94;

    private const char NoTile = 'x';

    private char[]? _announcedStacking;

    private volatile bool _stackingChanged;

    public string StackingMap()
    {
        char[] tiles = CurrentStacking();
        var map = new StringBuilder(tiles.Length + Grid.Height);
        for (int y = 0; y < Grid.Height; y++)
        {
            map.Append(tiles, y * Grid.Width, Grid.Width).Append('\r');
        }

        return map.ToString();
    }

    public string? TakeStackingMapUpdate()
    {
        if (!_stackingChanged)
        {
            return null;
        }

        _stackingChanged = false;
        char[] current = CurrentStacking();
        char[]? announced = _announcedStacking;
        _announcedStacking = current;

        var update = new StringBuilder();
        int skipped = 0;
        for (int i = 0; i < current.Length; i++)
        {
            if (current[i] == NoTile || (announced is not null && announced[i] == current[i]))
            {
                skipped++;
                continue;
            }

            AppendSkip(update, skipped);
            skipped = 0;
            update.Append(current[i]);
        }

        return update.Length == 0 ? null : update.ToString();
    }

    private static void AppendSkip(StringBuilder update, int tiles)
    {
        for (; tiles > 0; tiles -= MaxSkipRun)
        {
            update.Append('!').Append((char)(32 + Math.Min(tiles, MaxSkipRun)));
        }
    }

    private char[] CurrentStacking()
    {
        var tiles = new char[Grid.Width * Grid.Height];
        lock (_furnitureLock)
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                for (int x = 0; x < Grid.Width; x++)
                {
                    tiles[y * Grid.Width + x] = StackingTile(x, y);
                }
            }
        }

        return tiles;
    }

    private char StackingTile(int x, int y)
    {
        bool isDoor = Grid.IsDoor(x, y);
        if (!Grid.IsWalkable(x, y) && !isDoor)
        {
            return NoTile;
        }

        double top = Grid.HeightAt(x, y);
        bool blocked = isDoor || !Grid.CanHoldFurniture(x, y);
        foreach (RoomFloorItem item in ItemsAtUnlocked(x, y))
        {
            blocked |= !item.IsStackable;
            top = Math.Max(top, item.SurfaceZ);
        }

        int height = (int)Math.Clamp(Math.Round(top, MidpointRounding.AwayFromZero), 0, 9);
        return blocked ? (char)('a' + Math.Min(height, 5)) : (char)('0' + height);
    }
}
