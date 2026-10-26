using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Sound;

public sealed class PlayListMessageEvent : IMessageEvent
{
    private readonly int _positionSeconds;
    private readonly IReadOnlyList<TraxSong> _songs;

    public PlayListMessageEvent(int positionSeconds, IReadOnlyList<TraxSong> songs)
    {
        _positionSeconds = positionSeconds;
        _songs = songs;
    }

    public static PlayListMessageEvent Empty { get; } = new(0, []);

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Sound.PlayListMessageEvent);
        packet.AppendInt32(_positionSeconds);
        packet.AppendInt32(_songs.Count);
        foreach (TraxSong song in _songs)
        {
            packet.AppendInt32(song.Id);
            packet.AppendInt32(song.LengthSeconds);
            packet.AppendString(song.Name);
            packet.AppendString(song.Author);
        }

        return packet;
    }
}
