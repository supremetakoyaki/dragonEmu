namespace DragonEmu.Game.Rooms.Pathfinding;

public sealed class RoomGrid
{
    private readonly bool[,] _walkable;
    private readonly double[,] _height;
    private readonly (int X, int Y, double Z)? _door;
    private readonly HashSet<(int X, int Y)> _ladders = [];

    public int Width { get; }

    public int Height { get; }

    public double? MaxStepHeight { get; }

    public RoomGrid(string heightmap, (int X, int Y, double Z)? door = null, double? maxStepHeight = null, bool doorIsFloor = false)
    {
        _door = door;
        MaxStepHeight = maxStepHeight;
        string[] rows = heightmap.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        Height = rows.Length;
        Width = Height == 0 ? 0 : rows[0].Length;

        _walkable = new bool[Width, Height];
        _height = new double[Width, Height];

        for (int y = 0; y < Height; y++)
        {
            string row = rows[y];
            for (int x = 0; x < Width && x < row.Length; x++)
            {
                char tile = row[x];
                if (tile is >= '0' and <= '9')
                {
                    _walkable[x, y] = true;
                    _height[x, y] = tile - '0';
                }
            }
        }

        if (door is { } d && d.X >= 0 && d.X < Width && d.Y >= 0 && d.Y < Height)
        {
            _walkable[d.X, d.Y] = doorIsFloor;
            if (doorIsFloor)
            {
                _height[d.X, d.Y] = d.Z;
            }
        }
    }

    public bool IsWalkable(int x, int y) =>
        x >= 0 && x < Width && y >= 0 && y < Height && _walkable[x, y];

    public bool IsDoor(int x, int y) => _door is { } door && door.X == x && door.Y == y;

    public bool IsWalkGoal(int x, int y) => IsWalkable(x, y) || IsDoor(x, y);

    public bool CanHoldFurniture(int x, int y) => IsWalkable(x, y) && !IsDoor(x, y);

    public void AddLadder(int x, int y) => _ladders.Add((x, y));

    public bool CanStep((int X, int Y) from, (int X, int Y) to) =>
        MaxStepHeight is not { } max
        || Math.Abs(HeightAt(to.X, to.Y) - HeightAt(from.X, from.Y)) <= max
        || ((_ladders.Contains(from) || _ladders.Contains(to)) && (from.X == to.X || from.Y == to.Y));

    public double HeightAt(int x, int y) => !IsWalkable(x, y) && _door is { } door && IsDoor(x, y) ? door.Z : _height[x, y];
}
