namespace DragonEmu.Domain;

public enum NavigatorImageType
{
    Internal,
    External,
}

public enum NavigatorImageSize
{
    Square,
    Wide,
}

public sealed record NavigatorPublicRoom(
    int Id,
    int OrderNum,
    NavigatorImageSize ImageSize,
    string Caption,
    string Description,
    string Image,
    NavigatorImageType ImageType,
    int RoomId,
    int MaxUsers,
    int CategoryRef,
    bool ClubOnly,
    int NavIndex,
    bool Visible = true);

public static class NavigatorPublicRoomCategoryRefs
{
    public const int Root = -1;
}
