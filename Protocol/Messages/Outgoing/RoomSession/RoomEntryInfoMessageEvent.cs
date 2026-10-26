namespace DragonEmu.Protocol.Messages.Outgoing.RoomSession;

public sealed class RoomEntryInfoMessageEvent : IMessageEvent
{
    private readonly bool _isPrivate;
    private readonly int _roomId;
    private readonly bool _rights;
    private readonly string _venueCode;
    private readonly int _venueIndex;

    private RoomEntryInfoMessageEvent(bool isPrivate, int roomId, bool rights, string venueCode, int venueIndex)
    {
        _isPrivate = isPrivate;
        _roomId = roomId;
        _rights = rights;
        _venueCode = venueCode;
        _venueIndex = venueIndex;
    }

    public static RoomEntryInfoMessageEvent ForPrivateRoom(int roomId, bool rights) =>
        new(isPrivate: true, roomId, rights, venueCode: string.Empty, venueIndex: 0);

    public static RoomEntryInfoMessageEvent ForPublicRoom(string venueCode, int venueIndex) =>
        new(isPrivate: false, roomId: 0, rights: false, venueCode, venueIndex);

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.RoomEntryInfoMessageEvent);
        packet.AppendBoolean(_isPrivate);
        if (_isPrivate)
        {
            packet.AppendInt32(_roomId);
            packet.AppendBoolean(_rights);
        }
        else
        {
            packet.AppendString(_venueCode);
            packet.AppendInt32(_venueIndex);
        }

        return packet;
    }
}
