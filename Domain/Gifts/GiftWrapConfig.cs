namespace DragonEmu.Domain;

public sealed record GiftWrapBox(int FurniDefId, int FurnidataRef);

public sealed record GiftWrapConfig(
    IReadOnlyList<GiftWrapBox> Boxes,
    IReadOnlyList<int> ColourValues,
    IReadOnlyList<int> RibbonIds,
    int WrapPriceCredits);
