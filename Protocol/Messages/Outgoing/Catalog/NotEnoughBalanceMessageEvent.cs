namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public sealed class NotEnoughBalanceMessageEvent : IMessageEvent
{
    private readonly bool _notEnoughCredits;
    private readonly bool _notEnoughPixels;

    public NotEnoughBalanceMessageEvent(bool notEnoughCredits, bool notEnoughPixels)
    {
        _notEnoughCredits = notEnoughCredits;
        _notEnoughPixels = notEnoughPixels;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Catalog.NotEnoughBalanceMessageEvent);
        packet.AppendInt32(_notEnoughCredits ? 1 : 0);
        packet.AppendInt32(_notEnoughPixels ? 1 : 0);
        return packet;
    }
}
