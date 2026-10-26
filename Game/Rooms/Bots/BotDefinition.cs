namespace DragonEmu.Game.Rooms;

public enum BotTrigger
{
    UserSays,
    UserShouts,
    Random,

    UserChats,

    OnArrival,
}

public enum BotStepType
{
    Say,
    Shout,
    Whisper,
    Walk,
    HandItem,
    Wait,
    Wave,
    Dance,
    LookAt,
}

public sealed record BotStep(BotStepType Type, string Text = "", int Value = 0, (int X, int Y)? Tile = null);

public sealed record BotAction(
    int Id, BotTrigger Trigger, PhraseMatcher? Phrases, int MinTicks, int MaxTicks, int CooldownTicks, int TileDistance,
    IReadOnlyList<IReadOnlyList<BotStep>> Alternatives);

public sealed record BotDefinition(
    int Id, string Name, char Gender, string Figure, string Motto, int X, int Y, double Z, int Rotation,
    IReadOnlySet<(int X, int Y)>? WalkableArea, bool AllowDiagonal, IReadOnlyList<BotAction> Actions, bool IsGuide = false);
