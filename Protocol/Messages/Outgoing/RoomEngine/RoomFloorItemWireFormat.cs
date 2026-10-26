using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

internal static class RoomFloorItemWireFormat
{
    public static void AppendItem(ServerPacket packet, UserItem item)
    {
        packet.AppendInt32(item.Id);
        packet.AppendInt32(item.RefFurnidata);
        packet.AppendInt32(item.X ?? 0);
        packet.AppendInt32(item.Y ?? 0);
        packet.AppendInt32((item.Rotation ?? 0) / 45);
        packet.AppendString((item.Z ?? 0).ToString("0.0###"));
        packet.AppendInt32(item.Extras ?? 0);
        packet.AppendString(item.ExtraData ?? string.Empty);
        packet.AppendInt32(item.RentMinutesLeft(DateTime.UtcNow));
    }
}
