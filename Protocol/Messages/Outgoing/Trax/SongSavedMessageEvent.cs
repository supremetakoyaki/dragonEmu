namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class SongSavedMessageEvent : IMessageEvent
{
    private readonly int _songId;
    private readonly string _name;

    public SongSavedMessageEvent(int songId, string name)
    {
        _songId = songId;
        _name = name;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Trax.SongSavedMessageEvent);
        packet.AppendInt32(_songId);
        packet.AppendString(_name);
        return packet;
    }
}
