using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public sealed class CatalogPageMessageEvent : IMessageEvent
{
    private readonly CatalogPageBody _body;
    private readonly IReadOnlyList<CatalogOffer> _offers;

    public CatalogPageMessageEvent(CatalogPageBody body, IReadOnlyList<CatalogOffer> offers)
    {
        _body = body;
        _offers = offers;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Catalog.CatalogPageMessageEvent);

        packet.AppendInt32(_body.PageId);
        packet.AppendString(_body.LayoutCode);

        packet.AppendInt32(_body.LocalizationImages.Count);
        foreach (string image in _body.LocalizationImages)
        {
            packet.AppendString(image);
        }

        packet.AppendInt32(_body.LocalizationTexts.Count);
        foreach (string text in _body.LocalizationTexts)
        {
            packet.AppendString(text);
        }

        packet.AppendInt32(_offers.Count);
        foreach (CatalogOffer offer in _offers)
        {
            packet.AppendInt32(offer.Id);
            packet.AppendString(offer.OfferCode);
            packet.AppendInt32(offer.PriceCredits);
            packet.AppendInt32(offer.PriceActivityPoints);

            packet.AppendInt32(offer.Products.Count);
            foreach (CatalogOfferProduct product in offer.Products)
            {
                packet.AppendString(product.ProductType.ToWireChar());
                packet.AppendInt32(product.WireId);
                packet.AppendString(product.ExtraData ?? string.Empty);
                packet.AppendInt32(product.Quantity);
                packet.AppendInt32(product.RentalDuration);
            }
        }

        return packet;
    }
}
