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

    private ScrSendUserInfoEvent(string productName, int days, int months, int subscriptionType)
    {
        _productName = productName;
        _days = days;
        _months = months;
        _subscriptionType = subscriptionType;
    }

    public static ScrSendUserInfoEvent NotSubscribed(string productName) => new(productName, 0, 0, 0);

    public static ScrSendUserInfoEvent HabboClubStatus(DateTime clubExpiresAt)
    {
        (int days, int months) = DecomposeRemaining(clubExpiresAt);
        return new("habbo_club", days, months, NoReactionSubscriptionType);
    }

    public static ScrSendUserInfoEvent HabboClubJustSubscribed(DateTime clubExpiresAt)
    {
        (int days, int months) = DecomposeRemaining(clubExpiresAt);
        return new("habbo_club", days, months, FreshSubscriptionEventType);
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
        return packet;
    }
}
