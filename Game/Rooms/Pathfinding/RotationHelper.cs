namespace DragonEmu.Game.Rooms.Pathfinding;

public static class RotationHelper
{
    private static readonly byte[] Rotations =
    [
        7, 0, 1,
        6, 0, 2,
        5, 4, 3
    ];

    public static int Calculate(int fromX, int fromY, int toX, int toY)
    {
        int dx = Math.Sign(toX - fromX);
        int dy = Math.Sign(toY - fromY);

        return Rotations[((dy + 1) * 3) + dx + 1];
    }
}
