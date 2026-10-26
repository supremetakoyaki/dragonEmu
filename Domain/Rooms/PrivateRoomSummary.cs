namespace DragonEmu.Domain;

public sealed record PrivateRoomSummary(
    int Id,
    string Label,
    string Description,
    int CategoryId,
    int AccessType,
    string? Password,
    int? OwnerId,
    string OwnerUsername,
    int MaxUsers,
    bool AllowTrading,
    bool AllowPets,
    int Score,
    IReadOnlyList<string> Tags,
    int IconBackground,
    int IconForeground,
    IReadOnlyList<RoomIconItem> IconItems,
    string Heightmap,
    int DoorX,
    int DoorY,
    double DoorZ,
    int DoorDir,
    string Wallpaper,
    string Floor,
    string Landscape,
    string HeightmapLabel,
    bool AllowPetsEat = false,
    bool AllowWalkthrough = false,
    int QueuetileSpeed = 1,
    double? MaxStepHeight = null,
    double? PoolWaterMaxHeight = null)
{
    public const int MinQueuetileSpeed = 1;
    public const int MaxQueuetileSpeed = 4;

    public int RollerIntervalTicks => MaxQueuetileSpeed + 1 - Math.Clamp(QueuetileSpeed, MinQueuetileSpeed, MaxQueuetileSpeed);
}
