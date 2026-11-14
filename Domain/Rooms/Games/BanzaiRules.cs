namespace DragonEmu.Domain;

public static class BanzaiRules
{
    public const int MaxPlayersPerTeam = 5;

    public const int TileOff = 0;

    public const int TileLit = 1;

    public const int EffectBase = 32;

    public const int EndDelayTicks = 2;

    public const int BlinkFrames = 10;

    public static readonly IReadOnlyList<GameTeam> Teams = [GameTeam.Red, GameTeam.Green, GameTeam.Blue, GameTeam.Yellow];

    public static int IndexOf(GameTeam team) => team switch
    {
        GameTeam.Red => 1,
        GameTeam.Green => 2,
        GameTeam.Blue => 3,
        GameTeam.Yellow => 4,
        _ => 0,
    };

    public static GameTeam TeamOfIndex(int index) => index switch
    {
        1 => GameTeam.Red,
        2 => GameTeam.Green,
        3 => GameTeam.Blue,
        4 => GameTeam.Yellow,
        _ => GameTeam.None,
    };

    public static int EffectOf(GameTeam team) => team == GameTeam.None ? 0 : EffectBase + IndexOf(team);

    public static int TileState(GameTeam team, int step) => IndexOf(team) * 3 + step;

    public static int LockedState(GameTeam team) => TileState(team, 2);

    public static GameTeam TeamOfTile(int state) => state is >= 3 and <= 14 ? TeamOfIndex(state / 3) : GameTeam.None;

    public static bool IsLocked(int state) => state is >= 3 and <= 14 && state % 3 == 2;

    public static int Paint(int state, GameTeam team)
    {
        if (team == GameTeam.None || IsLocked(state))
        {
            return state;
        }

        return TeamOfTile(state) == team ? state + 1 : TileState(team, 0);
    }

    public static int PuckTrail(GameTeam team) => IndexOf(team);

    public static int BlinkFrame(int frame, int lockedState) => frame % 2 == 0 ? TileOff : lockedState;
}
