using System.Globalization;
using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class RoomEventEvent : IMessageEvent
{
    private readonly int _roomId;
    private readonly RoomEvent? _event;

    public RoomEventEvent(int roomId, RoomEvent? roomEvent)
    {
        _roomId = roomId;
        _event = roomEvent;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.RoomEventEvent);
        if (_event is not { } roomEvent)
        {
            packet.AppendString("-1");
            return packet;
        }

        packet.AppendString(roomEvent.HostUserId.ToString(CultureInfo.InvariantCulture));
        packet.AppendString(roomEvent.HostName);
        packet.AppendString(_roomId.ToString(CultureInfo.InvariantCulture));
        packet.AppendInt32(roomEvent.Category);
        packet.AppendString(roomEvent.Name);
        packet.AppendString(roomEvent.Description);
        packet.AppendString(roomEvent.StartTime);
        packet.AppendInt32(roomEvent.Tags.Count);
        foreach (string tag in roomEvent.Tags)
        {
            packet.AppendString(tag);
        }

        return packet;
    }
}
