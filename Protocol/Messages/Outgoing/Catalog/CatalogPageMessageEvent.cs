using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public sealed class CatalogPageMessageEvent : IMessageEvent
{
    private readonly CatalogPageBody _body;
    private readonly IReadOnlyList<CatalogOffer> _offers;
    private readonly Language _language;

    public CatalogPageMessageEvent(CatalogPageBody body, IReadOnlyList<CatalogOffer> offers, Language language)
    {
        _body = body;
        _offers = offers;
        _language = language;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Catalog.CatalogPageMessageEvent);

        packet.AppendInt32(_body.PageId);
        packet.AppendString(_body.LayoutCode);

        IReadOnlyList<string> images = _body.LocalizationImages.For(_language);
        packet.AppendInt32(images.Count);
        foreach (string image in images)
        {
            packet.AppendString(image);
        }

        IReadOnlyList<string> texts = _body.LocalizationTexts.For(_language);
        packet.AppendInt32(texts.Count);
        foreach (string text in texts)
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
