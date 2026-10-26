namespace DragonEmu.Domain;

public sealed record PublicRoomFurnitureItem(
    int Id,
    string Classname,
    int X,
    int Y,
    double Z,
    int Rotation,
    int XDim,
    int YDim,
    double ZDim,
    bool IsSittable,
    bool IsBed,
    bool IsWalkable,
    bool IsInvisible);
