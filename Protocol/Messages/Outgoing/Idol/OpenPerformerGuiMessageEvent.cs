using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Idol;

public sealed class OpenPerformerGuiMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<TraxSong> _songs;

    public OpenPerformerGuiMessageEvent(IReadOnlyList<TraxSong> songs)
    {
        _songs = songs;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Idol.OpenPerformerGuiMessageEvent);
        packet.AppendInt32(_songs.Count);
        foreach (TraxSong song in _songs)
        {
            packet.AppendInt32(song.Id);
            packet.AppendString(song.Name);
        }

        return packet;
    }
}
