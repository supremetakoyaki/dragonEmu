namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class RoomVisualizationSettingsEvent : IMessageEvent
{
    private readonly bool _wallsHidden;

    public RoomVisualizationSettingsEvent(bool wallsHidden)
    {
        _wallsHidden = wallsHidden;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.RoomVisualizationSettingsEvent);
        packet.AppendBoolean(_wallsHidden);
        return packet;
    }
}
