namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class SongListFullMessageEvent : IMessageEvent
{
    private readonly int _limit;

    public SongListFullMessageEvent(int limit)
    {
        _limit = limit;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Trax.SongListFullMessageEvent);
        packet.AppendInt32(_limit);
        return packet;
    }
}
