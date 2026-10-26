namespace DragonEmu.Protocol.Messages.Outgoing.RoomSettings;

public sealed class RoomSettingsSavedEvent : IMessageEvent
{
    private readonly int _roomId;

    public RoomSettingsSavedEvent(int roomId)
    {
        _roomId = roomId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomSettings.RoomSettingsSavedEvent);
        packet.AppendInt32(_roomId);
        return packet;
    }
}
