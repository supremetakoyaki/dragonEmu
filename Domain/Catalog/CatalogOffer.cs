namespace DragonEmu.Domain;

public sealed record PrebuiltSong(string Name, string Author, string Data);

public sealed record CatalogOffer(
    int Id,
    int RefPage,
    string OfferCode,
    int PriceCredits,
    int PriceActivityPoints,
    bool ClubOnly,
    bool Giftable,
    IReadOnlyList<CatalogOfferProduct> Products,
    PrebuiltSong? PrebuiltSong = null)
{
    public bool IsActuallyGiftable =>
        Giftable
        && Products.Count == 1
        && Products[0].ProductType is CatalogProductType.Floor or CatalogProductType.Wall or CatalogProductType.Subscription or CatalogProductType.Film or CatalogProductType.Effect
        && (Products[0].RentalDuration <= 0 || Products[0].ProductType == CatalogProductType.Effect)
        && PetOffer.TypeOf(this) is null
        && PrebuiltSong is null;
}
