using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

internal static class RoomWallItemWireFormat
{
    public static void AppendItem(ServerPacket packet, UserItem item)
    {
        packet.AppendString(item.Id.ToString());
        packet.AppendInt32(item.RefFurnidata);
        packet.AppendString(item.WallPosition ?? string.Empty);
        packet.AppendString(item.LogicClass == FurnitureLogicClass.Stickie ? Stickie.ColourOf(item.ExtraData) : item.ExtraData ?? string.Empty);
    }
}
