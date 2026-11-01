using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class JukeboxDisksMessageEvent : IMessageEvent
{
    private readonly int _slots;
    private readonly IReadOnlyList<JukeboxDisk> _disks;
    private readonly int _positionSeconds;
    private readonly IReadOnlyList<int> _playlist;

    public JukeboxDisksMessageEvent(int slots, IReadOnlyList<JukeboxDisk> disks, int positionSeconds, IReadOnlyList<int> playlist)
    {
        _slots = slots;
        _disks = disks;
        _positionSeconds = positionSeconds;
        _playlist = playlist;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Trax.JukeboxDisksMessageEvent);
        packet.AppendInt32(_slots);
        packet.AppendInt32(_disks.Count);
        foreach (JukeboxDisk disk in _disks)
        {
            packet.AppendInt32(disk.Slot);
            packet.AppendInt32(disk.Song.Id);
            packet.AppendInt32(disk.Song.LengthSeconds);
            packet.AppendString(disk.Song.Name);
            packet.AppendString(disk.Song.Author);
        }

        packet.AppendInt32(_positionSeconds);
        packet.AppendInt32(_playlist.Count);
        foreach (int songId in _playlist)
        {
            packet.AppendInt32(songId);
        }

        return packet;
    }
}
