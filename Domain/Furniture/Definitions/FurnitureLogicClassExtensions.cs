namespace DragonEmu.Domain;

public static class FurnitureLogicClassExtensions
{
    public static string ToDbString(this FurnitureLogicClass value) => value switch
    {
        FurnitureLogicClass.Basic => "basic",
        FurnitureLogicClass.Multistate => "multistate",
        FurnitureLogicClass.Trophy => "trophy",
        FurnitureLogicClass.Window => "window",
        FurnitureLogicClass.Present => "present",
        FurnitureLogicClass.SoundMachine => "sound_machine",
        FurnitureLogicClass.Credit => "credit",
        FurnitureLogicClass.OneWayDoor => "one_way_door",
        FurnitureLogicClass.RoomDimmer => "roomdimmer",
        FurnitureLogicClass.Stickie => "stickie",
        FurnitureLogicClass.Dice => "dice",
        FurnitureLogicClass.HabboWheel => "habbowheel",
        FurnitureLogicClass.Jukebox => "jukebox",
        FurnitureLogicClass.RandomState => "randomstate",
        FurnitureLogicClass.HockeyScore => "hockey_score",
        FurnitureLogicClass.PlanetSystem => "planet_system",
        FurnitureLogicClass.EcotronBox => "ecotron_box",
        FurnitureLogicClass.VotingChair => "voting_chair",
        FurnitureLogicClass.Hotspot => "hotspot",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown furniture logic class."),
    };

    public static FurnitureLogicClass FromDbString(string value) => value switch
    {
        "basic" => FurnitureLogicClass.Basic,
        "multistate" => FurnitureLogicClass.Multistate,
        "trophy" => FurnitureLogicClass.Trophy,
        "window" => FurnitureLogicClass.Window,
        "present" => FurnitureLogicClass.Present,
        "sound_machine" => FurnitureLogicClass.SoundMachine,
        "credit" => FurnitureLogicClass.Credit,
        "one_way_door" => FurnitureLogicClass.OneWayDoor,
        "roomdimmer" => FurnitureLogicClass.RoomDimmer,
        "stickie" => FurnitureLogicClass.Stickie,
        "dice" => FurnitureLogicClass.Dice,
        "habbowheel" => FurnitureLogicClass.HabboWheel,
        "jukebox" => FurnitureLogicClass.Jukebox,
        "randomstate" => FurnitureLogicClass.RandomState,
        "hockey_score" => FurnitureLogicClass.HockeyScore,
        "planet_system" => FurnitureLogicClass.PlanetSystem,
        "ecotron_box" => FurnitureLogicClass.EcotronBox,
        "voting_chair" => FurnitureLogicClass.VotingChair,
        "hotspot" => FurnitureLogicClass.Hotspot,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown furniture logic_class value from the database."),
    };
}
