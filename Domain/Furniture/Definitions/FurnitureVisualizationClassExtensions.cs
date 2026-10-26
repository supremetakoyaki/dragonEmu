namespace DragonEmu.Domain;

public static class FurnitureVisualizationClassExtensions
{
    public static string ToDbString(this FurnitureVisualizationClass value) => value switch
    {
        FurnitureVisualizationClass.Static => "static",
        FurnitureVisualizationClass.Animated => "animated",
        FurnitureVisualizationClass.QueueTile => "queue_tile",
        FurnitureVisualizationClass.GiftWrapped => "gift_wrapped",
        FurnitureVisualizationClass.HabboWheel => "habbowheel",
        FurnitureVisualizationClass.Bottle => "bottle",
        FurnitureVisualizationClass.ValRandomizer => "val_randomizer",
        FurnitureVisualizationClass.PartyBeamer => "party_beamer",
        FurnitureVisualizationClass.PlanetSystem => "planet_system",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown furniture visualization class."),
    };

    public static FurnitureVisualizationClass FromDbString(string value) => value switch
    {
        "static" => FurnitureVisualizationClass.Static,
        "animated" => FurnitureVisualizationClass.Animated,
        "queue_tile" => FurnitureVisualizationClass.QueueTile,
        "gift_wrapped" => FurnitureVisualizationClass.GiftWrapped,
        "habbowheel" => FurnitureVisualizationClass.HabboWheel,
        "bottle" => FurnitureVisualizationClass.Bottle,
        "val_randomizer" => FurnitureVisualizationClass.ValRandomizer,
        "party_beamer" => FurnitureVisualizationClass.PartyBeamer,
        "planet_system" => FurnitureVisualizationClass.PlanetSystem,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown furniture visualization_class value from the database."),
    };
}
