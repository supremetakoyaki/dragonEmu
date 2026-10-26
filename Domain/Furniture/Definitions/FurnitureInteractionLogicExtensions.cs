namespace DragonEmu.Domain;

public static class FurnitureInteractionLogicExtensions
{
    public static string ToDbString(this FurnitureInteractionLogic value) => value switch
    {
        FurnitureInteractionLogic.None => "none",
        FurnitureInteractionLogic.Toggle => "toggle",
        FurnitureInteractionLogic.Teleport => "teleport",
        FurnitureInteractionLogic.Vending => "vending",
        FurnitureInteractionLogic.Gate => "gate",
        FurnitureInteractionLogic.Roller => "roller",
        FurnitureInteractionLogic.PetFood => "petfood",
        FurnitureInteractionLogic.PetNest => "petnest",
        FurnitureInteractionLogic.PetWater => "petwater",
        FurnitureInteractionLogic.PetToy => "pettoy",
        FurnitureInteractionLogic.PetTreat => "pettreat",
        FurnitureInteractionLogic.TotemLeg => "totem_leg",
        FurnitureInteractionLogic.TotemHead => "totem_head",
        FurnitureInteractionLogic.TotemPlanet => "totem_planet",
        FurnitureInteractionLogic.HockeyLight => "hockey_light",
        FurnitureInteractionLogic.SoundDisk => "sound_disk",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown furniture interaction logic."),
    };

    public static FurnitureInteractionLogic FromDbString(string value) => value switch
    {
        "none" => FurnitureInteractionLogic.None,
        "toggle" => FurnitureInteractionLogic.Toggle,
        "teleport" => FurnitureInteractionLogic.Teleport,
        "vending" => FurnitureInteractionLogic.Vending,
        "gate" => FurnitureInteractionLogic.Gate,
        "roller" => FurnitureInteractionLogic.Roller,
        "petfood" => FurnitureInteractionLogic.PetFood,
        "petnest" => FurnitureInteractionLogic.PetNest,
        "petwater" => FurnitureInteractionLogic.PetWater,
        "pettoy" => FurnitureInteractionLogic.PetToy,
        "pettreat" => FurnitureInteractionLogic.PetTreat,
        "totem_leg" => FurnitureInteractionLogic.TotemLeg,
        "totem_head" => FurnitureInteractionLogic.TotemHead,
        "totem_planet" => FurnitureInteractionLogic.TotemPlanet,
        "hockey_light" => FurnitureInteractionLogic.HockeyLight,
        "sound_disk" => FurnitureInteractionLogic.SoundDisk,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown furniture interaction_logic value from the database."),
    };
}
