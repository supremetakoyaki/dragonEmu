using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Avatar;

public sealed class WardrobeMessageEvent : IMessageEvent
{
    private readonly bool _canUse;
    private readonly IReadOnlyList<WardrobeOutfit> _outfits;

    public WardrobeMessageEvent(bool canUse, IReadOnlyList<WardrobeOutfit> outfits)
    {
        _canUse = canUse;
        _outfits = outfits;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Avatar.WardrobeMessageEvent);
        packet.AppendInt32(_canUse ? 1 : 0);
        packet.AppendInt32(_outfits.Count);
        foreach (WardrobeOutfit outfit in _outfits)
        {
            packet.AppendInt32(outfit.Slot);
            packet.AppendString(outfit.Figure);
            packet.AppendString(outfit.Gender.ToString());
        }

        return packet;
    }
}
