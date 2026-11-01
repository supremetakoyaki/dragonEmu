namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class JukeboxPlaylistFullMessageEvent : IMessageEvent
{
    private readonly int _limit;

    public JukeboxPlaylistFullMessageEvent(int limit)
    {
        _limit = limit;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Trax.JukeboxPlaylistFullMessageEvent);
        packet.AppendInt32(_limit);
        return packet;
    }
}
