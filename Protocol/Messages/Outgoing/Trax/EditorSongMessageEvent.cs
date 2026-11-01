using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class EditorSongMessageEvent : IMessageEvent
{
    private readonly TraxSong _song;

    public EditorSongMessageEvent(TraxSong song)
    {
        _song = song;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Trax.EditorSongMessageEvent);
        packet.AppendInt32(_song.Id);
        packet.AppendString(_song.Name);
        packet.AppendString(_song.Data);
        return packet;
    }
}
