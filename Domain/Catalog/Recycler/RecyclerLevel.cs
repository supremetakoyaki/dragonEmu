namespace DragonEmu.Domain;

public sealed record RecyclerPrize(int FurniDefId, FurnitureType Type, int RefFurnidata);

public sealed record RecyclerLevel(int Level, int Odds, IReadOnlyList<RecyclerPrize> Prizes);
