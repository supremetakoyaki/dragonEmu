using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Marketplace;

public sealed class MarketplaceCancelOfferResultEvent : IMessageEvent
{
    private readonly int _offerId;
    private readonly bool _cancelled;

    public MarketplaceCancelOfferResultEvent(int offerId, bool cancelled)
    {
        _offerId = offerId;
        _cancelled = cancelled;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Marketplace.MarketplaceCancelOfferResultEvent);
        packet.AppendInt32(_offerId);
        packet.AppendInt32(_cancelled ? 1 : 0);
        return packet;
    }
}
