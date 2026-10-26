namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public sealed class GiftWrappingConfigurationEvent : IMessageEvent
{
    private readonly bool _giftWrappingAvailable;
    private readonly int _priceCredits;
    private readonly IReadOnlyList<int> _boxIds;
    private readonly IReadOnlyList<int> _colourValues;
    private readonly IReadOnlyList<int> _ribbonIds;

    public GiftWrappingConfigurationEvent(
        bool giftWrappingAvailable,
        int priceCredits,
        IReadOnlyList<int> boxIds,
        IReadOnlyList<int> colourValues,
        IReadOnlyList<int> ribbonIds)
    {
        _giftWrappingAvailable = giftWrappingAvailable;
        _priceCredits = priceCredits;
        _boxIds = boxIds;
        _colourValues = colourValues;
        _ribbonIds = ribbonIds;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Catalog.GiftWrappingConfigurationEvent);
        packet.AppendBoolean(_giftWrappingAvailable);
        packet.AppendInt32(_priceCredits);
        AppendIntArray(packet, _boxIds);
        AppendIntArray(packet, _colourValues);
        AppendIntArray(packet, _ribbonIds);
        return packet;
    }

    private static void AppendIntArray(ServerPacket packet, IReadOnlyList<int> values)
    {
        packet.AppendInt32(values.Count);
        foreach (int value in values)
        {
            packet.AppendInt32(value);
        }
    }
}
