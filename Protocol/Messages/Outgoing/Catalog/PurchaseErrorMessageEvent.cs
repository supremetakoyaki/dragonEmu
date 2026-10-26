namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public enum PurchaseErrorReason
{
    Generic = 0,
    OfferNotFound = 3,
    OfferNotPurchasable = 4,
}

public sealed class PurchaseErrorMessageEvent : IMessageEvent
{
    private readonly PurchaseErrorReason _reason;

    public PurchaseErrorMessageEvent(PurchaseErrorReason reason)
    {
        _reason = reason;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Catalog.PurchaseErrorMessageEvent);
        packet.AppendInt32((int)_reason);
        return packet;
    }
}
