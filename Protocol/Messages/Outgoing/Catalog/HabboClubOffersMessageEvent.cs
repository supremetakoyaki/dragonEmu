namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public sealed class HabboClubOffersMessageEvent : IMessageEvent
{
    public sealed record Offer(
        int OfferId, string ProductCode, int Price, bool Upgrade, bool Vip, int Periods, int DaysLeftAfterPurchase, int Year, int Month, int Day);

    private readonly IReadOnlyList<Offer> _offers;

    public HabboClubOffersMessageEvent(IReadOnlyList<Offer> offers)
    {
        _offers = offers;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Catalog.HabboClubOffersMessageEvent);
        packet.AppendInt32(_offers.Count);
        foreach (Offer offer in _offers)
        {
            packet.AppendInt32(offer.OfferId);
            packet.AppendString(offer.ProductCode);
            packet.AppendInt32(offer.Price);
            packet.AppendBoolean(offer.Upgrade);
            packet.AppendBoolean(offer.Vip);
            packet.AppendInt32(offer.Periods);
            packet.AppendInt32(offer.DaysLeftAfterPurchase);
            packet.AppendInt32(offer.Year);
            packet.AppendInt32(offer.Month);
            packet.AppendInt32(offer.Day);
        }

        return packet;
    }
}
