using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Marketplace;

public sealed class MarketplaceConfigurationEvent : IMessageEvent
{
    private readonly bool _enabled;
    private readonly MarketplaceConfig _config;

    public MarketplaceConfigurationEvent(bool enabled, MarketplaceConfig config)
    {
        _enabled = enabled;
        _config = config;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Marketplace.MarketplaceConfigurationEvent);
        packet.AppendBoolean(_enabled);
        packet.AppendInt32(_config.CommissionPercent);
        packet.AppendInt32(_config.TokenBatchPrice);
        packet.AppendInt32(_config.TokenBatchSize);
        packet.AppendInt32(_config.OfferMinPrice);
        packet.AppendInt32(_config.OfferMaxPrice);
        packet.AppendInt32(_config.OfferExpirationHours);
        packet.AppendInt32(_config.AveragePricePeriodDays);
        return packet;
    }
}
