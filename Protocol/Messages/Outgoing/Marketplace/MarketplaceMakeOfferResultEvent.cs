using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Marketplace;

public sealed class MarketplaceMakeOfferResultEvent : IMessageEvent
{
    private readonly MarketplaceMakeOfferResult _result;

    public MarketplaceMakeOfferResultEvent(MarketplaceMakeOfferResult result)
    {
        _result = result;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Marketplace.MarketplaceMakeOfferResult);
        packet.AppendInt32((int)_result);
        return packet;
    }
}
