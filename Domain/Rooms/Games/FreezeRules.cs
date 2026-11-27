using System.Globalization;

namespace DragonEmu.Domain;

public enum FreezeBallKind
{
    Normal,
    Diagonal,
    Mega,
}

public static class FreezeRules
{
    public const int MaxPlayersPerTeam = 5;

    public const int EffectBase = 39;

    public const int ShieldEffectBase = 48;

    public const int FrozenEffect = 12;

    public const int FrozenEffectDelayMs = 100;

    public const int OutEffect = 4;

    public const int FuseTicks = 4;

    public const int FrozenTicks = 10;

    public const int ShieldTicks = 10;

    public const int ShieldPowerUpTicks = 19;

    public const int ExitTeleportTicks = 4;

    public const int OutEffectTicks = 3;

    public const int ThrowCooldownTicks = 2;

    public const int EndDelayTicks = 1;

    public const int StartReach = 2;

    public const int StartLives = 3;

    public const int MaxLives = 3;

    public const int PointsPerLife = 10;

    public const int PointsPerFreeze = 10;

    public const int MegaReach = 64;

    public const int MaxDelayStep = 9;

    public const int TilePlain = 0;
    public const int TileSnowball = 1;
    public const int TileMegaSnowball = 6;
    public const int TileBlast = 11;

    public const int BlockWhole = 0;
    public const int BlockEmpty = 1;

    public const int PowerUpBiggerBlast = 2;
    public const int PowerUpMoreSnowballs = 3;
    public const int PowerUpDiagonalBlast = 4;
    public const int PowerUpMegaSnowball = 5;
    public const int PowerUpLife = 6;
    public const int PowerUpShield = 7;

    public const int EmptyWeight = 2;

    public const int ExitOff = 0;
    public const int ExitLit = 1;

    public const string FighterAchievement = "ACH_EsA";
    public const string PlayerAchievement = "ACH_FreezePlayer";
    public const string WinnerAchievement = "ACH_FreezeWinner";
    public const string PowerUpAchievement = "ACH_FreezePowerUp";

    public static readonly IReadOnlyList<GameTeam> Teams = BanzaiRules.Teams;

    public static int EffectOf(GameTeam team) => team == GameTeam.None ? 0 : EffectBase + BanzaiRules.IndexOf(team);

    public static int ShieldEffectOf(GameTeam team) => team == GameTeam.None ? 0 : ShieldEffectBase + BanzaiRules.IndexOf(team);

    public static string Shown(int state, int delayStep = 0) =>
        (state * 1000 + Math.Clamp(delayStep, 0, MaxDelayStep) * 100).ToString(CultureInfo.InvariantCulture);

    public static bool IsPowerUp(int blockState) => blockState is >= PowerUpBiggerBlast and <= PowerUpShield;

    public static bool IsWhole(int blockState) => blockState == BlockWhole;

    public static int Taken(int blockState) => 10 + blockState;

    public static int BrokenContent(int roll) => roll < EmptyWeight ? BlockEmpty : PowerUpBiggerBlast + (roll - EmptyWeight);

    public const int RollRange = EmptyWeight + 6;

    public static IReadOnlyList<(int X, int Y)> Directions(FreezeBallKind kind) => kind switch
    {
        FreezeBallKind.Diagonal => Diagonals,
        FreezeBallKind.Mega => [.. Sides, .. Diagonals],
        _ => Sides,
    };

    private static readonly (int X, int Y)[] Sides = [(0, -1), (1, 0), (0, 1), (-1, 0)];
    private static readonly (int X, int Y)[] Diagonals = [(1, -1), (1, 1), (-1, 1), (-1, -1)];

    public static int DelayStep(int distance, int reachedAt) => reachedAt <= 0 ? Math.Max(1, distance) : reachedAt + distance;

    public static bool InThrowingReach(int playerX, int playerY, int x, int y) => Math.Abs(playerX - x) <= 1 && Math.Abs(playerY - y) <= 1;

    public static IReadOnlySet<GameTeam> Winners(IReadOnlyDictionary<GameTeam, int> scores)
    {
        int best = scores.Count == 0 ? 0 : scores.Values.Max();
        return best <= 0 ? new HashSet<GameTeam>() : scores.Where(score => score.Value == best).Select(score => score.Key).ToHashSet();
    }
}
