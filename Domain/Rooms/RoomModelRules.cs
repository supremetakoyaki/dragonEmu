namespace DragonEmu.Domain;

public static class RoomModelRules
{
    public const string HabboClubFuse = "fuse_use_special_room_layouts";
    public const string VipFuse = "fuse_use_vip_room_layouts";

    private static readonly HashSet<string> ClientModels =
    [
        .. Enumerable.Range('a', 'r' - 'a' + 1).Select(letter => "model_" + (char)letter),
        .. Enumerable.Range('t', 'z' - 't' + 1).Select(letter => "model_" + (char)letter),
        "model_0",
    ];

    public static bool IsOfferedByClient(string label) => ClientModels.Contains(label);

    public static string? FuseFor(int clubLevel) => clubLevel switch
    {
        <= 0 => null,
        1 => HabboClubFuse,
        _ => VipFuse,
    };
}
