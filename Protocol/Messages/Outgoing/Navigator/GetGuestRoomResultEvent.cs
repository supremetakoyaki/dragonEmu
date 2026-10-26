using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class GetGuestRoomResultEvent : IMessageEvent
{
    private readonly bool _enterRoom;
    private readonly PrivateRoomSummary _room;
    private readonly int _usersNow;
    private readonly bool _roomForward;

    public GetGuestRoomResultEvent(bool enterRoom, PrivateRoomSummary room, int usersNow, bool roomForward)
    {
        _enterRoom = enterRoom;
        _room = room;
        _usersNow = usersNow;
        _roomForward = roomForward;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.GetGuestRoomResultEvent);
        packet.AppendBoolean(_enterRoom);
        GuestRoomDataWireFormat.Append(packet, _room, _usersNow);
        packet.AppendBoolean(_roomForward);
        return packet;
    }
}
