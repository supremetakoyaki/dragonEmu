using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

internal static class GuestRoomDataWireFormat
{
    public static void Append(ServerPacket packet, PrivateRoomSummary room, int usersNow, RoomEvent? roomEvent = null)
    {
        packet.AppendUInt32((uint)room.Id);
        packet.AppendBoolean(roomEvent is not null);
        packet.AppendString(roomEvent?.Name ?? room.Label);
        packet.AppendString(room.OwnerUsername);
        packet.AppendInt32(room.AccessType);
        packet.AppendInt32(usersNow);
        packet.AppendInt32(room.MaxUsers);
        packet.AppendString(roomEvent?.Description ?? room.Description);
        packet.AppendInt32(0);
        packet.AppendBoolean(room.AllowTrading);
        packet.AppendInt32(room.Score);
        packet.AppendInt32(roomEvent?.Category ?? room.CategoryId);
        packet.AppendString(roomEvent?.StartTime ?? string.Empty);

        IReadOnlyList<string> tags = roomEvent?.Tags ?? room.Tags;
        packet.AppendInt32(tags.Count);
        foreach (string tag in tags)
        {
            packet.AppendString(tag);
        }

        packet.AppendInt32(room.IconBackground);
        packet.AppendInt32(room.IconForeground);
        packet.AppendInt32(room.IconItems.Count);

        foreach (RoomIconItem item in room.IconItems)
        {
            packet.AppendInt32(item.Position);
            packet.AppendInt32(item.ItemId);
        }

        packet.AppendBoolean(room.AllowPets);
    }
}
