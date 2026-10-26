using DragonEmu.Domain;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;

internal static class UserItemWireFormat
{
    public const int PosterCategory = 6;

    private static readonly HashSet<FurnitureLogicClass> NonStackableLogicClasses =
        [FurnitureLogicClass.Trophy, FurnitureLogicClass.SoundMachine, FurnitureLogicClass.Jukebox, FurnitureLogicClass.EcotronBox, FurnitureLogicClass.Present];

    public static int CategoryOf(UserItem item) => RoomPaintService.PaintTypeFor(item.Classname) switch
    {
        RoomPaintType.Wallpaper => 2,
        RoomPaintType.Floor => 3,
        RoomPaintType.Landscape => 4,
        _ when item.LogicClass == FurnitureLogicClass.Stickie => 5,
        _ when item.Classname == "poster" => PosterCategory,
        _ => 0,
    };

    public static bool IsGroupable(UserItem item, int category) =>
        category is not (2 or 3 or 4) && !NonStackableLogicClasses.Contains(item.LogicClass);

    public static void AppendItem(ServerPacket packet, UserItem item)
    {
        packet.AppendInt32(item.Id);
        packet.AppendInt32(0);

        string wireType = item.Type == FurnitureType.Floor ? "S" : "I";
        packet.AppendString(wireType);

        int category = CategoryOf(item);
        bool allowInventoryStack = IsGroupable(item, category);

        packet.AppendInt32(item.Id);
        packet.AppendInt32(item.RefFurnidata);
        packet.AppendInt32(category);
        packet.AppendString(item.ExtraData ?? string.Empty);
        bool owned = item.ExpiresAt is null;
        packet.AppendBoolean(item.AllowRecycle && owned);
        packet.AppendBoolean(item.AllowTrade && owned);
        packet.AppendBoolean(allowInventoryStack);
        packet.AppendBoolean(item.AllowTrade && owned);
        packet.AppendInt32(item.RentMinutesLeft(DateTime.UtcNow));

        if (wireType == "S")
        {
            packet.AppendString(string.Empty);
            packet.AppendInt32(item.Extras ?? -1);
        }
    }
}
