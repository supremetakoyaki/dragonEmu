namespace DragonEmu.Protocol.Messages.Outgoing.RoomAction;

public sealed class DanceMessageEvent : IMessageEvent
{
    private readonly int _virtualId;
    private readonly int _danceId;

    public DanceMessageEvent(int virtualId, int danceId)
    {
        _virtualId = virtualId;
        _danceId = danceId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomAction.DanceMessageEvent);
        packet.AppendInt32(_virtualId);
        packet.AppendInt32(_danceId);
        return packet;
    }
}
