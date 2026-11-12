using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Users;

public sealed class ScrSendUserInfoEvent : IMessageEvent
{
    private const int NoReactionSubscriptionType = 0;
    private const int FreshSubscriptionEventType = 2;
    private const int DaysPerMonth = 30;

    private readonly string _productName;
    private readonly int _days;
    private readonly int _months;
    private readonly int _subscriptionType;
    private readonly ClubDetails _details;

    private ScrSendUserInfoEvent(string productName, int days, int months, int subscriptionType, ClubDetails details)
    {
        _productName = productName;
        _days = days;
        _months = months;
        _subscriptionType = subscriptionType;
        _details = details;
    }

    public readonly record struct ClubDetails(bool IsVip = false, int PastClubDays = 0, int PastVipDays = 0, bool ShowBasicPromo = false, int BasicNormalPrice = 0, int BasicPromoPrice = 0)
    {
        public static ClubDetails Of((bool IsVip, ClubGiftRules.Standing Standing) summary) =>
            new(summary.IsVip, ClubGiftRules.PastClubDays(summary.Standing), ClubGiftRules.PastVipDays(summary.Standing));
    }

    public static ScrSendUserInfoEvent NotSubscribed(string productName, ClubDetails details = default) => new(productName, 0, 0, 0, details);

    public static ScrSendUserInfoEvent HabboClubStatus(DateTime clubExpiresAt, ClubDetails details = default)
    {
        (int days, int months) = DecomposeRemaining(clubExpiresAt);
        return new("habbo_club", days, months, NoReactionSubscriptionType, details);
    }

    public static ScrSendUserInfoEvent HabboClubJustSubscribed(DateTime clubExpiresAt, ClubDetails details = default)
    {
        (int days, int months) = DecomposeRemaining(clubExpiresAt);
        return new("habbo_club", days, months, FreshSubscriptionEventType, details);
    }

    private static (int Days, int Months) DecomposeRemaining(DateTime clubExpiresAt)
    {
        int totalDaysRemaining = Math.Max(0, (int)Math.Ceiling((clubExpiresAt - DateTime.UtcNow).TotalDays));
        return totalDaysRemaining == 0
            ? (0, 0)
            : ((totalDaysRemaining - 1) % DaysPerMonth + 1, (totalDaysRemaining - 1) / DaysPerMonth);
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Users.ScrSendUserInfoEvent);
        packet.AppendString(_productName.ToLowerInvariant());
        packet.AppendInt32(_days);
        packet.AppendInt32(0);
        packet.AppendInt32(_months);
        packet.AppendInt32(_subscriptionType);
        packet.AppendBoolean(false);
        packet.AppendBoolean(_details.IsVip);
        packet.AppendInt32(_details.PastClubDays);
        packet.AppendInt32(_details.PastVipDays);
        packet.AppendBoolean(_details.ShowBasicPromo);
        packet.AppendInt32(_details.BasicNormalPrice);
        packet.AppendInt32(_details.BasicPromoPrice);
        return packet;
    }
}
