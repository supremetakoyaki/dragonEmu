namespace DragonEmu.Domain;

public enum TotemGlow
{
    None,
    Red,
    Green,
    Blue,
}

public static class Totem
{
    public const int LegStates = 12;
    public const int HeadLooks = 3;
    public const int PlanetStates = 3;

    private const int LegLookStates = 4;

    public static TotemGlow GlowOf(int legState) => (TotemGlow)Wrap(legState, LegLookStates);

    public static int NextLegState(int legState) => Wrap(legState + 1, LegStates);

    public static int HeadLook(int headState) => headState switch
    {
        >= 0 and < HeadLooks => headState,
        >= 4 and <= 14 when headState % 4 != 3 => headState / 4 - 1,
        _ => 0,
    };

    public static int HeadState(int look, TotemGlow glow) => glow == TotemGlow.None ? look : 4 * (look + 1) + (int)glow - 1;

    public static int NextHeadLook(int headState) => (HeadLook(headState) + 1) % HeadLooks;

    public static int NextPlanetState(int planetState) => Wrap(planetState + 1, PlanetStates);

    private static int Wrap(int value, int count) => ((value % count) + count) % count;
}
