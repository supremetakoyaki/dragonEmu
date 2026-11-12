using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public sealed class ClubGiftInfoEvent : IMessageEvent
{
    private readonly int _daysUntilNextGift;
    private readonly int _giftsAvailableNow;
    private readonly IReadOnlyList<ClubGiftDefinition> _gifts;
    private readonly Func<ClubGiftDefinition, bool> _isSelectable;
    private readonly Func<ClubGiftDefinition, int> _daysRequired;

    public ClubGiftInfoEvent(int daysUntilNextGift, int giftsAvailableNow, IReadOnlyList<ClubGiftDefinition> gifts, Func<ClubGiftDefinition, bool> isSelectable,
        Func<ClubGiftDefinition, int>? daysRequired = null)
    {
        _daysUntilNextGift = daysUntilNextGift;
        _giftsAvailableNow = giftsAvailableNow;
        _gifts = gifts;
        _isSelectable = isSelectable;
        _daysRequired = daysRequired ?? (gift => gift.MonthsRequired * ClubGiftRules.DaysPerMonth);
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Catalog.ClubGiftInfoEvent);

        packet.AppendInt32(_daysUntilNextGift);
        packet.AppendInt32(_giftsAvailableNow);

        packet.AppendInt32(_gifts.Count);
        foreach (ClubGiftDefinition gift in _gifts)
        {
            packet.AppendInt32(gift.Id);
            packet.AppendString(gift.OfferCode);
            packet.AppendInt32(0);
            packet.AppendInt32(0);

            packet.AppendInt32(1);
            packet.AppendString(gift.Product.ProductType.ToWireChar());
            packet.AppendInt32(gift.Product.WireId);
            packet.AppendString(gift.Product.ExtraData ?? string.Empty);
            packet.AppendInt32(gift.Product.Quantity);
            packet.AppendInt32(gift.Product.RentalDuration);
        }

        packet.AppendInt32(_gifts.Count);
        foreach (ClubGiftDefinition gift in _gifts)
        {
            packet.AppendInt32(gift.Id);
            packet.AppendBoolean(gift.Vip);
            packet.AppendInt32(_daysRequired(gift));
            packet.AppendBoolean(_isSelectable(gift));
        }

        return packet;
    }
}
