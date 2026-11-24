namespace DragonEmu.Domain;

public static class IceTagRules
{
    public const int SkatingEffectMale = 38;

    public const int SkatingEffectFemale = 39;

    public const int TaggedEffectMale = 45;

    public const int TaggedEffectFemale = 46;

    public static int EffectOf(char gender, bool tagged) => (gender == 'M', tagged) switch
    {
        (true, false) => SkatingEffectMale,
        (false, false) => SkatingEffectFemale,
        (true, true) => TaggedEffectMale,
        (false, true) => TaggedEffectFemale,
    };

    public const long TagCooldownMs = 3 * 500;

    public const double MaxHeightAboveIce = 0.2;

    public static bool OnIce(double z, double iceZ) => z - iceZ <= MaxHeightAboveIce + 0.001;

    public static bool CanTag(int x1, int y1, int x2, int y2) => Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2)) == 1;

    public const long MinuteMs = 60_000;

    public const string RinkBuilderAchievement = "ACH_TagA";

    public const string CaughtAchievement = "ACH_TagB";

    public const string TimeOnIceAchievement = "ACH_TagC";
}
