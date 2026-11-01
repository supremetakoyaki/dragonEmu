using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class RoomMusicMessageEvent : IMessageEvent
{
    private readonly int _positionSeconds;
    private readonly IReadOnlyList<TraxSong> _songs;

    public RoomMusicMessageEvent(int positionSeconds, IReadOnlyList<TraxSong> songs)
    {
        _positionSeconds = positionSeconds;
        _songs = songs;
    }

    public static RoomMusicMessageEvent Silence { get; } = new(0, []);

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Trax.RoomMusicMessageEvent);
        packet.AppendInt32(_positionSeconds);
        packet.AppendInt32(_songs.Count);
        foreach (TraxSong song in _songs)
        {
            packet.AppendInt32(song.Id);
            packet.AppendInt32(song.LengthSeconds);
            packet.AppendString(song.Data);
        }

        return packet;
    }
}
