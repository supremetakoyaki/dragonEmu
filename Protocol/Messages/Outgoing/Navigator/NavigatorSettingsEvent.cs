namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class NavigatorSettingsEvent : IMessageEvent
{
    private readonly int _homeRoomId;

    public NavigatorSettingsEvent(int homeRoomId = 0)
    {
        _homeRoomId = homeRoomId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.NavigatorSettingsEvent);
        packet.AppendInt32(_homeRoomId);
        return packet;
    }
}
