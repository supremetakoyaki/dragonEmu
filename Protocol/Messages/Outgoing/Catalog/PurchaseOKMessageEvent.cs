using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public sealed class PurchaseOKMessageEvent : IMessageEvent
{
    private readonly CatalogOffer _offer;

    public PurchaseOKMessageEvent(CatalogOffer offer)
    {
        _offer = offer;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Catalog.PurchaseOKMessageEvent);

        packet.AppendInt32(_offer.Id);
        packet.AppendString(_offer.OfferCode);
        packet.AppendInt32(_offer.PriceCredits);
        packet.AppendInt32(_offer.PriceActivityPoints);

        packet.AppendInt32(_offer.Products.Count);
        foreach (CatalogOfferProduct product in _offer.Products)
        {
            packet.AppendString(product.ProductType.ToWireChar());
            packet.AppendInt32(product.WireId);
            packet.AppendString(product.ExtraData ?? string.Empty);
            packet.AppendInt32(product.Quantity);
            packet.AppendInt32(product.RentalDuration);
        }

        return packet;
    }
}
