namespace DragonEmu.Protocol.Messages.Outgoing.RoomSettings;

public sealed class RoomSettingsErrorEvent : IMessageEvent
{
    public const int ErrorCode = 2;

    private readonly int _roomId;
    private readonly int _errorCode;

    public RoomSettingsErrorEvent(int roomId, int errorCode = ErrorCode)
    {
        _roomId = roomId;
        _errorCode = errorCode;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomSettings.RoomSettingsErrorEvent);
        packet.AppendInt32(_roomId);
        packet.AppendInt32(_errorCode);
        return packet;
    }
}
