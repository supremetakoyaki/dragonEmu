using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public sealed class ClubGiftSelectedEvent : IMessageEvent
{
    private readonly string _offerCode;
    private readonly CatalogOfferProduct _product;

    public ClubGiftSelectedEvent(string offerCode, CatalogOfferProduct product)
    {
        _offerCode = offerCode;
        _product = product;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Catalog.ClubGiftSelectedEvent);

        packet.AppendString(_offerCode);
        packet.AppendInt32(1);
        packet.AppendString(_product.ProductType.ToWireChar());
        packet.AppendInt32(_product.WireId);
        packet.AppendString(_product.ExtraData ?? string.Empty);
        packet.AppendInt32(_product.Quantity);
        packet.AppendInt32(_product.RentalDuration);

        return packet;
    }
}
