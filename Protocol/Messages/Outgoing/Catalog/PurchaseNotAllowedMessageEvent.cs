namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public enum PurchaseNotAllowedReason
{
    Unknown = 0,
    HabboClubRequired = 1,
}

public sealed class PurchaseNotAllowedMessageEvent : IMessageEvent
{
    private readonly PurchaseNotAllowedReason _reason;

    public PurchaseNotAllowedMessageEvent(PurchaseNotAllowedReason reason)
    {
        _reason = reason;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Catalog.PurchaseNotAllowedMessageEvent);
        packet.AppendInt32((int)_reason);
        return packet;
    }
}
