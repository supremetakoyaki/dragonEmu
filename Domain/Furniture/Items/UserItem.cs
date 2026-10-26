namespace DragonEmu.Domain;

public sealed record UserItem(
    int Id,
    int UserId,
    int FurniDefId,
    int RefFurnidata,
    string Classname,
    FurnitureType Type,
    FurnitureLogicClass LogicClass,
    bool AllowTrade,
    bool AllowRecycle,
    string? ExtraData,
    int? RoomRef,
    int? X,
    int? Y,
    decimal? Z,
    int? Rotation,
    string? WallPosition,
    bool Seen,
    int? Extras,
    DateTime? ExpiresAt = null,
    bool OnMarketplace = false)
{
    public int RentMinutesLeft(DateTime utcNow) =>
        ExpiresAt is { } expiresAt ? (int)Math.Max(0, Math.Ceiling((expiresAt - utcNow).TotalMinutes)) : -1;
}

public sealed record ExpiredRental(int ItemId, int UserId, int? RoomId, FurnitureType Type);
