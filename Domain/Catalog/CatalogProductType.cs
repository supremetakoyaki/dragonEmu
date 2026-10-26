namespace DragonEmu.Domain;

public enum CatalogProductType
{
    Floor,
    Wall,
    Effect,
    Subscription,
}

public static class CatalogProductTypeExtensions
{
    public static string ToWireChar(this CatalogProductType type) => type switch
    {
        CatalogProductType.Floor => "s",
        CatalogProductType.Wall => "i",
        CatalogProductType.Effect => "e",
        CatalogProductType.Subscription => "h",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown catalog product type."),
    };

    public static CatalogProductType FromWireChar(char wireChar) => wireChar switch
    {
        's' => CatalogProductType.Floor,
        'i' => CatalogProductType.Wall,
        'e' => CatalogProductType.Effect,
        'h' => CatalogProductType.Subscription,
        _ => throw new ArgumentOutOfRangeException(nameof(wireChar), wireChar, "Unknown catalog product type wire char."),
    };
}
