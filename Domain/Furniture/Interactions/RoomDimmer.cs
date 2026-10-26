using System.Globalization;

namespace DragonEmu.Domain;

public sealed record DimmerPreset(int Id, int Type, string Color, int Intensity);

public readonly record struct RoomDimmerState(bool IsOn, int PresetId, int Type, string Color, int Intensity)
{
    public static RoomDimmerState Basal { get; } = new(false, 1, 1, "#000000", 255);

    public string ToExtraData() => string.Create(CultureInfo.InvariantCulture, $"{(IsOn ? 2 : 1)},{PresetId},{Type},{Color},{Intensity}");

    public static RoomDimmerState Parse(string? extraData)
    {
        string[] parts = (extraData ?? string.Empty).Split(',');
        if (parts.Length != 5
            || !int.TryParse(parts[0], out int state) || state is not (1 or 2)
            || !int.TryParse(parts[1], out int presetId) || !int.TryParse(parts[2], out int type) || !int.TryParse(parts[4], out int intensity)
            || !RoomDimmers.IsValidPreset(presetId, type, parts[3], intensity))
        {
            return Basal;
        }

        return new RoomDimmerState(state == 2, presetId, type, parts[3], intensity);
    }

    public RoomDimmerState WithPreset(DimmerPreset preset) =>
        this with { PresetId = preset.Id, Type = preset.Type, Color = preset.Color, Intensity = preset.Intensity };
}

public static class RoomDimmers
{
    public const int PresetCount = 3;

    private static readonly HashSet<string> Colors = new(StringComparer.OrdinalIgnoreCase)
    {
        "#74F5F5", "#0053F7", "#E759DE", "#EA4532", "#F2F851", "#82F349", "#000000",
    };

    public static bool IsValidPreset(int presetId, int type, string color, int intensity) =>
        presetId is >= 1 and <= PresetCount && type is 1 or 2 && Colors.Contains(color) && intensity is >= 0 and <= 255;

    public static DimmerPreset DefaultPreset(int presetId) => new(presetId, 1, "#000000", 255);
}
