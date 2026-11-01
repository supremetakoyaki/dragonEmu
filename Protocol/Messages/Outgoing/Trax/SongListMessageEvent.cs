using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class SongListMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<TraxSongEntry> _songs;
    private readonly IReadOnlyList<int> _playlist;

    public SongListMessageEvent(IReadOnlyList<TraxSongEntry> songs, IReadOnlyList<int> playlist)
    {
        _songs = songs;
        _playlist = playlist;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Trax.SongListMessageEvent);
        packet.AppendInt32(_songs.Count);
        foreach (TraxSongEntry song in _songs)
        {
            packet.AppendInt32(song.Id);
            packet.AppendInt32(song.LengthSeconds);
            packet.AppendString(song.Name);
            packet.AppendBoolean(song.Burnt);
        }

        packet.AppendInt32(_playlist.Count);
        foreach (int songId in _playlist)
        {
            packet.AppendInt32(songId);
        }

        return packet;
    }
}
