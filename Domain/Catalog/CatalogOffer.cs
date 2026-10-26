namespace DragonEmu.Domain;

public sealed record CatalogOffer(
    int Id,
    int RefPage,
    string OfferCode,
    int PriceCredits,
    int PriceActivityPoints,
    bool ClubOnly,
    bool Giftable,
    IReadOnlyList<CatalogOfferProduct> Products)
{
    public bool IsActuallyGiftable =>
        Giftable
        && Products.Count == 1
        && Products[0].ProductType is CatalogProductType.Floor or CatalogProductType.Wall or CatalogProductType.Subscription
        && Products[0].RentalDuration <= 0
        && PetOffer.TypeOf(this) is null;
}
