namespace DragonEmu.Domain;

public static class CurrencyType
{
    public const int Credits = 1;
    public const int ActivityPoints = 5; // pixels

    public const int Film = 99;

    private const int OtherActivityPoints = 100;

    public const int Snowflakes = OtherActivityPoints + ActivityPointType.Snowflakes;

    public static int OfActivityPoints(int activityPointType) =>
        activityPointType == ActivityPointType.Pixels ? ActivityPoints : OtherActivityPoints + activityPointType;
}

public static class ActivityPointType
{
    public const int Pixels = 0;
    public const int Snowflakes = 1;

    public static readonly IReadOnlyList<int> All = [Pixels, Snowflakes];
}
