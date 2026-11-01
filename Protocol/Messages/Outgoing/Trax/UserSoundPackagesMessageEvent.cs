using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class UserSoundPackagesMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<TraxSoundSet> _sets;

    public UserSoundPackagesMessageEvent(IReadOnlyList<TraxSoundSet> sets)
    {
        _sets = sets;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Trax.UserSoundPackagesMessageEvent);
        packet.AppendInt32(_sets.Count);
        foreach (TraxSoundSet set in _sets)
        {
            packet.AppendInt32(set.Id);
            packet.AppendString(set.Name);
        }

        return packet;
    }
}
