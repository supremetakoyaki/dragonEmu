using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Marketplace;

public sealed class MarketPlaceOffersEvent : IMessageEvent
{
    private readonly IReadOnlyList<MarketplaceListing> _listings;

    public MarketPlaceOffersEvent(IReadOnlyList<MarketplaceListing> listings)
    {
        _listings = listings;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Marketplace.MarketPlaceOffersEvent);
        packet.AppendInt32(_listings.Count);
        foreach (MarketplaceListing listing in _listings)
        {
            packet.AppendInt32(listing.OfferId);
            packet.AppendInt32((int)MarketplaceOfferStatus.Open);
            packet.AppendInt32(listing.FurniType);
            packet.AppendInt32(listing.Sprite);
            packet.AppendInt32(listing.Price);
            packet.AppendInt32(listing.MinutesLeft);
            packet.AppendInt32(listing.AveragePrice);
            packet.AppendInt32(listing.OfferCount);
        }

        return packet;
    }
}
