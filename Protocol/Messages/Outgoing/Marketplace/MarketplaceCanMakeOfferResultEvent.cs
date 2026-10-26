using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Marketplace;

public sealed class MarketplaceCanMakeOfferResultEvent : IMessageEvent
{
    private readonly MarketplaceCanMakeOfferResult _result;
    private readonly int _tokens;

    public MarketplaceCanMakeOfferResultEvent(MarketplaceCanMakeOfferResult result, int tokens)
    {
        _result = result;
        _tokens = tokens;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Marketplace.MarketplaceCanMakeOfferResult);
        packet.AppendInt32((int)_result);
        packet.AppendInt32(_tokens);
        return packet;
    }
}
