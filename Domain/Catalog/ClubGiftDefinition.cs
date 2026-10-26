namespace DragonEmu.Domain;

public sealed record ClubGiftDefinition(
    int Id,
    string OfferCode,
    int MonthsRequired,
    CatalogOfferProduct Product);
