namespace DragonEmu.Domain;

public static class FootballRules
{
    public static readonly IReadOnlyList<string> KickStates = ["55", "44", "33", "22", "11", "11"];

    public const string IcePuckClassname = "es_puck";

    public static readonly IReadOnlyList<string> IcePuckKickStates = ["88", "77", "66", "55", "44", "33", "22", "11", "11"];

    public static int Bounce(int direction, bool xBlocked, bool yBlocked)
    {
        (int dx, int dy) = Delta(direction);
        if (dx == 0 || dy == 0 || xBlocked == yBlocked)
        {
            return Opposite(direction);
        }

        return xBlocked ? DirectionOf(-dx, dy) : DirectionOf(dx, -dy);
    }

    public static (int Dx, int Dy) Delta(int direction)
    {
        (int x, int y) = Step(0, 0, direction);
        return (x, y);
    }

    public static int DirectionOf(int dx, int dy) => (dx, dy) switch
    {
        (0, -1) => 0,
        (1, -1) => 1,
        (1, 0) => 2,
        (1, 1) => 3,
        (0, 1) => 4,
        (-1, 1) => 5,
        (-1, 0) => 6,
        _ => 7,
    };

    public const string RollingSlowly = "11";

    public static string RollState(string state, int? trail) => trail is { } digit ? $"{state[0]}{digit}" : state;

    public static int IntervalMs(string state) => 500 / Math.Max(1, (state[0] - '0'));

    public const int MaxScore = 99;

    public static int NextScore(int score) => score >= MaxScore ? 0 : Math.Max(0, score) + 1;

    public static int PreviousScore(int score) => score <= 0 ? MaxScore : Math.Min(MaxScore, score) - 1;

    public static readonly IReadOnlyList<int> CountdownPresets = [30, 60, 120, 180, 300, 600];

    public static int NextCountdownPreset(int seconds)
    {
        for (int i = 0; i < CountdownPresets.Count; i++)
        {
            if (CountdownPresets[i] == seconds)
            {
                return CountdownPresets[(i + 1) % CountdownPresets.Count];
            }
        }

        return CountdownPresets[0];
    }

    public static CounterButton CounterButtonOf(int param) => param switch
    {
        1 => CounterButton.Add,
        2 => CounterButton.Take,
        0 or 3 => CounterButton.Reset,
        _ => CounterButton.None,
    };

    public static bool MayChangePreset(bool running) => !running;

    public const int CountdownTicksPerSecond = 2;

    public static int Opposite(int direction) => (direction + 4) % 8;

    public static (int X, int Y) Step(int x, int y, int direction) => direction switch
    {
        0 => (x, y - 1),
        1 => (x + 1, y - 1),
        2 => (x + 1, y),
        3 => (x + 1, y + 1),
        4 => (x, y + 1),
        5 => (x - 1, y + 1),
        6 => (x - 1, y),
        _ => (x - 1, y - 1),
    };

    public static bool EntersGoal(int ballDirection, int goalDirection) => ballDirection == Opposite(goalDirection);

    public static bool LeavesGoal(int ballDirection, int goalDirection) => ballDirection == goalDirection;

    public const long GoalCooldownMs = 6 * 500;
}

public enum CounterButton
{
    None,
    Add,
    Take,
    Reset,
}
