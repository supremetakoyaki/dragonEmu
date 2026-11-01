using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class SoundPackagesMessageEvent : IMessageEvent
{
    private readonly int _slots;
    private readonly IReadOnlyList<(int Slot, TraxSoundSet Set)> _sets;
    private readonly IReadOnlyDictionary<int, int> _bars;

    public SoundPackagesMessageEvent(int slots, IReadOnlyList<(int Slot, TraxSoundSet Set)> sets, IReadOnlyDictionary<int, int> bars)
    {
        _slots = slots;
        _sets = sets;
        _bars = bars;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Trax.SoundPackagesMessageEvent);
        packet.AppendInt32(_slots);
        packet.AppendInt32(_sets.Count);
        foreach ((int slot, TraxSoundSet set) in _sets)
        {
            packet.AppendInt32(slot);
            packet.AppendInt32(set.Id);
            packet.AppendString(set.Name);
            packet.AppendInt32(set.SampleIds.Count);
            foreach (int sampleId in set.SampleIds)
            {
                packet.AppendInt32(sampleId);
                packet.AppendInt32(_bars.GetValueOrDefault(sampleId));
            }
        }

        return packet;
    }
}
