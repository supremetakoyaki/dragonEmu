namespace DragonEmu.Protocol.Messages.Outgoing.RoomAction;

public sealed class WaveMessageEvent : IMessageEvent
{
    private readonly int _virtualId;

    public WaveMessageEvent(int virtualId)
    {
        _virtualId = virtualId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomAction.WaveMessageEvent);
        packet.AppendInt32(_virtualId);
        return packet;
    }
}
