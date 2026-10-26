namespace DragonEmu.Game.Rooms.Pathfinding;

public static class AStarPathfinder
{
    private static readonly (int Dx, int Dy)[] Directions =
    [
        (1, 0), (-1, 0), (0, 1), (0, -1),
        (1, 1), (1, -1), (-1, 1), (-1, -1),
    ];

    public static IReadOnlyList<(int X, int Y)>? FindPath(
        RoomGrid grid,
        IReadOnlySet<(int X, int Y)> occupied,
        (int X, int Y) start,
        (int X, int Y) goal,
        IReadOnlySet<(int X, int Y)>? blockedUnlessGoal = null,
        IReadOnlySet<(int X, int Y)>? blockedAsGoal = null,
        IReadOnlySet<(int X, int Y)>? allowedTiles = null,
        bool allowDiagonal = true)
    {
        if (!grid.IsWalkGoal(goal.X, goal.Y) || occupied.Contains(goal) || blockedAsGoal?.Contains(goal) == true
            || allowedTiles?.Contains(goal) == false)
        {
            return null;
        }

        var open = new PriorityQueue<(int X, int Y), double>();
        var cameFrom = new Dictionary<(int X, int Y), (int X, int Y)>();
        var bestCost = new Dictionary<(int X, int Y), double> { [start] = 0 };
        var closed = new HashSet<(int X, int Y)>();

        open.Enqueue(start, Heuristic(start, goal));

        while (open.TryDequeue(out (int X, int Y) current, out _))
        {
            if (!closed.Add(current))
            {
                continue;
            }

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            foreach ((int dx, int dy) in Directions)
            {
                (int X, int Y) neighbor = (current.X + dx, current.Y + dy);

                bool isDiagonal = dx != 0 && dy != 0;
                if (isDiagonal && !allowDiagonal)
                {
                    continue;
                }

                bool steppable = grid.IsWalkable(neighbor.X, neighbor.Y) || (neighbor == goal && grid.IsWalkGoal(neighbor.X, neighbor.Y));
                if (!steppable || !grid.CanStep(current, neighbor) || occupied.Contains(neighbor) || allowedTiles?.Contains(neighbor) == false
                    || (neighbor != goal && blockedUnlessGoal is not null && blockedUnlessGoal.Contains(neighbor)))
                {
                    continue;
                }

                if (isDiagonal)
                {
                    (int X, int Y) sideA = (current.X + dx, current.Y);
                    (int X, int Y) sideB = (current.X, current.Y + dy);

                    if (!grid.IsWalkable(sideA.X, sideA.Y) || !grid.IsWalkable(sideB.X, sideB.Y) ||
                        occupied.Contains(sideA) || occupied.Contains(sideB))
                    {
                        continue;
                    }
                }

                double tentativeCost = bestCost[current] + (isDiagonal ? Math.Sqrt(2) : 1.0);

                if (!bestCost.TryGetValue(neighbor, out double existingCost) || tentativeCost < existingCost)
                {
                    bestCost[neighbor] = tentativeCost;
                    cameFrom[neighbor] = current;
                    open.Enqueue(neighbor, tentativeCost + Heuristic(neighbor, goal));
                }
            }
        }

        return null;
    }

    private static double Heuristic((int X, int Y) a, (int X, int Y) b)
    {
        int dx = Math.Abs(a.X - b.X);
        int dy = Math.Abs(a.Y - b.Y);
        return Math.Max(dx, dy) + (Math.Sqrt(2) - 1) * Math.Min(dx, dy);
    }

    private static List<(int X, int Y)> ReconstructPath(
        Dictionary<(int X, int Y), (int X, int Y)> cameFrom, (int X, int Y) current)
    {
        var path = new List<(int X, int Y)> { current };
        while (cameFrom.TryGetValue(current, out (int X, int Y) previous))
        {
            current = previous;
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
}
