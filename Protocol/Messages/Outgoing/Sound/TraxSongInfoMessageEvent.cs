using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Sound;

public sealed class TraxSongInfoMessageEvent : IMessageEvent
{
    private readonly TraxSong _song;

    public TraxSongInfoMessageEvent(TraxSong song)
    {
        _song = song;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Sound.TraxSongInfoMessageEvent);
        packet.AppendInt32(_song.Id);
        packet.AppendString(_song.Name);
        packet.AppendString(_song.Data);
        return packet;
    }
}
