namespace DragonEmu.Domain;

public static class FootballRules
{
    public static readonly IReadOnlyList<string> KickStates = ["55", "44", "33", "22", "11", "11"];

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
}

public enum CounterButton
{
    None,
    Add,
    Take,
    Reset,
}
