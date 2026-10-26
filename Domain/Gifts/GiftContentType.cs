namespace DragonEmu.Domain;

public enum GiftContentType
{
    Furni,
    Effect,
    Subscription,
}

public static class GiftContentTypeExtensions
{
    public static string ToJsonString(this GiftContentType value) => value switch
    {
        GiftContentType.Furni => "furni",
        GiftContentType.Effect => "effect",
        GiftContentType.Subscription => "subscription",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown gift content type."),
    };

    public static GiftContentType FromJsonString(string value) => value switch
    {
        "furni" => GiftContentType.Furni,
        "effect" => GiftContentType.Effect,
        "subscription" => GiftContentType.Subscription,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown gift content type from the database."),
    };
}
