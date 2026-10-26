using System.Globalization;
using DragonEmu.Domain;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Game.Furniture;

public enum FurnitureRollKind
{
    None,
    Dice,
    Bottle,
    Wheel,
    ValRandomizer,
    Fortune,
}

public static class FurnitureRolls
{
    private const int DiceRollTicks = 3;
    private const int BottleSpinTicks = 3;
    private const int WheelSpinTicks = 10;
    private const int ValRandomizerSpinTicks = 7;
    private const int FortuneSpinTicks = 4;
    private const int RandomizerResultTicks = 20;
    private const string FortuneSpinning = "8";

    public static FurnitureRollKind KindOf(FurnitureLogicClass logicClass, FurnitureVisualizationClass visualizationClass) =>
        logicClass switch
        {
            FurnitureLogicClass.Dice when visualizationClass == FurnitureVisualizationClass.Bottle => FurnitureRollKind.Bottle,
            FurnitureLogicClass.Dice => FurnitureRollKind.Dice,
            FurnitureLogicClass.HabboWheel => FurnitureRollKind.Wheel,
            FurnitureLogicClass.RandomState when visualizationClass == FurnitureVisualizationClass.ValRandomizer => FurnitureRollKind.ValRandomizer,
            FurnitureLogicClass.RandomState => FurnitureRollKind.Fortune,
            _ => FurnitureRollKind.None,
        };

    public static string? BasalExtraData(FurnitureRollKind kind) => kind switch
    {
        FurnitureRollKind.Dice or FurnitureRollKind.Bottle => "0",
        FurnitureRollKind.Wheel or FurnitureRollKind.ValRandomizer or FurnitureRollKind.Fortune => "-1",
        _ => null,
    };

    public static bool ResetsWhenPlaced(FurnitureRollKind kind) =>
        kind is FurnitureRollKind.Wheel or FurnitureRollKind.ValRandomizer or FurnitureRollKind.Fortune;

    public const int RandomizerResultShown = 2;

    public static bool ResetsWhileShowingResult(FurnitureRollKind kind) => kind == FurnitureRollKind.Fortune;

    public static (IReadOnlyList<RollStep> Steps, string? RestingExtraData) Plan(FurnitureRollKind kind, Random random)
    {
        switch (kind)
        {
            case FurnitureRollKind.Dice:
                return Spin(DiceRollTicks, random.Next(1, 7));
            case FurnitureRollKind.Bottle:
                return Spin(BottleSpinTicks, random.Next(0, 8));
            case FurnitureRollKind.Wheel:
                return Spin(WheelSpinTicks, random.Next(1, 11));
            case FurnitureRollKind.ValRandomizer:
                return Randomize("0", ValRandomizerSpinTicks, random.Next(1, 5));
            case FurnitureRollKind.Fortune:
                return Randomize(FortuneSpinning, FortuneSpinTicks, random.Next(0, 8));
            default:
                return ([], null);
        }
    }

    private static (IReadOnlyList<RollStep>, string?) Spin(int spinTicks, int result)
    {
        string value = result.ToString(CultureInfo.InvariantCulture);
        return ([new RollStep(0, "-1"), new RollStep(spinTicks, value)], value);
    }

    private static (IReadOnlyList<RollStep>, string?) Randomize(string spinning, int spinTicks, int result) =>
        ([
            new RollStep(0, spinning),
            new RollStep(spinTicks, result.ToString(CultureInfo.InvariantCulture)),
            new RollStep(RandomizerResultTicks, "-1"),
        ], "-1");
}
