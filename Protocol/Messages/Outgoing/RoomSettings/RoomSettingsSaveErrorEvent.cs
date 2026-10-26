namespace DragonEmu.Protocol.Messages.Outgoing.RoomSettings;

public sealed class RoomSettingsSaveErrorEvent : IMessageEvent
{
    public const int NameMandatory = 7;

    public const int PasswordMandatory = 5;

    public const int UnacceptableWordsInName = 8;

    public const int UnacceptableWordsInDescription = 10;

    public const int UnacceptableWordsInTag = 11;

    public const int NonUserChoosableTag = 12;

    public const int Forbidden = 2;

    private readonly int _roomId;
    private readonly int _errorCode;
    private readonly string _info;

    public RoomSettingsSaveErrorEvent(int roomId, int errorCode, string info = "")
    {
        _roomId = roomId;
        _errorCode = errorCode;
        _info = info;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomSettings.RoomSettingsSaveErrorEvent);
        packet.AppendInt32(_roomId);
        packet.AppendInt32(_errorCode);
        packet.AppendString(_info);
        return packet;
    }
}
