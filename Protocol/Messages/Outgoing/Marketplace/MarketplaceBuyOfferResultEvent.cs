using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Marketplace;

public sealed class MarketplaceBuyOfferResultEvent : IMessageEvent
{
    private readonly MarketplacePurchase _purchase;
    private readonly int _requestedOfferId;

    public MarketplaceBuyOfferResultEvent(MarketplacePurchase purchase, int requestedOfferId)
    {
        _purchase = purchase;
        _requestedOfferId = requestedOfferId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Marketplace.MarketplaceBuyOfferResultEvent);
        packet.AppendInt32((int)_purchase.Result);
        packet.AppendInt32(_purchase.OfferId);
        packet.AppendInt32(_purchase.Price);
        packet.AppendInt32(_requestedOfferId);
        return packet;
    }
}
