using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class UserSongDisksMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<TraxDiskItem> _disks;

    public UserSongDisksMessageEvent(IReadOnlyList<TraxDiskItem> disks)
    {
        _disks = disks;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Trax.UserSongDisksMessageEvent);
        packet.AppendInt32(_disks.Count);
        foreach (TraxDiskItem disk in _disks)
        {
            packet.AppendInt32(disk.ItemId);
            packet.AppendString(disk.Song.Name);
            packet.AppendString(disk.Song.Author);
        }

        return packet;
    }
}
