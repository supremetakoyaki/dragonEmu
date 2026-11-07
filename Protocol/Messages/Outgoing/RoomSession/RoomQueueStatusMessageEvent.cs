using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomSession;

public sealed class RoomQueueStatusMessageEvent : IMessageEvent
{
    private readonly RoomQueueStatus _status;
    private readonly bool _canChangeLine;

    public RoomQueueStatusMessageEvent(RoomQueueStatus status, bool canChangeLine)
    {
        _status = status;
        _canChangeLine = canChangeLine;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomSession.RoomQueueStatusMessageEvent);
        packet.AppendInt32(_canChangeLine ? 2 : 1);
        AppendQueue(packet, _status.Place);
        if (_canChangeLine)
        {
            AppendQueue(packet, _status.OtherLine);
        }

        return packet;
    }

    private static void AppendQueue(ServerPacket packet, RoomQueuePlace place)
    {
        packet.AppendString(place.Line == RoomQueueLine.Visitors ? "visitors" : "spectators");
        packet.AppendInt32((int)place.Line);
        packet.AppendInt32(place.ClubAhead is null ? 1 : 2);
        packet.AppendString("d");
        packet.AppendInt32(place.Ahead);
        if (place.ClubAhead is { } clubAhead)
        {
            packet.AppendString("c");
            packet.AppendInt32(clubAhead);
        }
    }
}
