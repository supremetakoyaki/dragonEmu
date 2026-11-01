namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class PlayListInvalidMessageEvent : IMessageEvent
{
    private readonly int _limit;

    public PlayListInvalidMessageEvent(int limit)
    {
        _limit = limit;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Trax.PlayListInvalidMessageEvent);
        packet.AppendInt32(_limit);
        return packet;
    }
}
