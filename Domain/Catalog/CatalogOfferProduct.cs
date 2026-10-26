namespace DragonEmu.Domain;

public sealed record CatalogOfferProduct(
    CatalogProductType ProductType,
    int WireId,
    string? ExtraData,
    int Quantity,
    int RentalDuration,
    int? FurnitureDefId);
