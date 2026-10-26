using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Marketplace;

public sealed class MarketPlaceOwnOffersEvent : IMessageEvent
{
    private readonly MarketplaceOwnOffers _own;

    public MarketPlaceOwnOffersEvent(MarketplaceOwnOffers own)
    {
        _own = own;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Marketplace.MarketPlaceOwnOffersEvent);
        packet.AppendInt32(_own.CreditsWaiting);
        packet.AppendInt32(_own.Offers.Count);
        foreach (MarketplaceOwnOffer offer in _own.Offers)
        {
            packet.AppendInt32(offer.OfferId);
            packet.AppendInt32((int)offer.Status);
            packet.AppendInt32(offer.FurniType);
            packet.AppendInt32(offer.Sprite);
            packet.AppendInt32(offer.Price);
            packet.AppendInt32(offer.MinutesLeft);
            packet.AppendInt32(offer.AveragePrice);
        }

        return packet;
    }
}
