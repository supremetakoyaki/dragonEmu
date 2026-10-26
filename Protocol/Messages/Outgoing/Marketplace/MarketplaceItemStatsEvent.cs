using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Marketplace;

public sealed class MarketplaceItemStatsEvent : IMessageEvent
{
    private readonly MarketplaceItemStats _stats;

    public MarketplaceItemStatsEvent(MarketplaceItemStats stats)
    {
        _stats = stats;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Marketplace.MarketplaceItemStatsEvent);
        packet.AppendInt32(_stats.AveragePrice);
        packet.AppendInt32(_stats.OfferCount);
        packet.AppendInt32(_stats.HistoryDays);
        packet.AppendInt32(_stats.Days.Count);
        foreach (MarketplaceDay day in _stats.Days)
        {
            packet.AppendInt32(day.DayOffset);
            packet.AppendInt32(day.AveragePrice);
            packet.AppendInt32(day.Sold);
        }

        packet.AppendInt32(_stats.FurniType);
        packet.AppendInt32(_stats.Sprite);
        return packet;
    }
}
