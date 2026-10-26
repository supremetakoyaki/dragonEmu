namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public sealed class IsOfferGiftableMessageEvent : IMessageEvent
{
    private readonly int _offerId;
    private readonly bool _isGiftable;

    public IsOfferGiftableMessageEvent(int offerId, bool isGiftable)
    {
        _offerId = offerId;
        _isGiftable = isGiftable;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Catalog.IsOfferGiftableMessageEvent);
        packet.AppendInt32(_offerId);
        packet.AppendBoolean(_isGiftable);
        return packet;
    }
}
