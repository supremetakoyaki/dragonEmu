namespace DragonEmu.Domain;

public static class ClubGiftRules
{
    public const int DaysPerMonth = 31;

    public readonly record struct Standing(int ClubMonths, int ClubGiftsClaimed, int VipDays, int VipGiftsClaimed);

    public static int PastClubDays(Standing standing) => standing.ClubMonths * DaysPerMonth;

    public static int PastVipDays(Standing standing) => standing.VipDays;

    public static int VipMonths(Standing standing) => standing.VipDays / DaysPerMonth;

    public static int ClubPicks(Standing standing) => Math.Max(0, standing.ClubMonths - standing.ClubGiftsClaimed);

    public static int VipPicks(Standing standing) => Math.Max(0, VipMonths(standing) - standing.VipGiftsClaimed);

    public static int PicksShown(Standing standing, ClubMembership membership) =>
        ClubPicks(standing) + (membership == ClubMembership.Vip ? VipPicks(standing) : 0);

    public static int DaysShown(ClubGiftDefinition gift, Standing standing, ClubMembership membership)
    {
        int days = gift.MonthsRequired * DaysPerMonth;
        return gift.Vip && membership != ClubMembership.Vip ? Math.Max(days, PastVipDays(standing) + 1) : days;
    }

    public static bool HasTime(ClubGiftDefinition gift, Standing standing) =>
        (gift.Vip ? PastVipDays(standing) : PastClubDays(standing)) >= gift.MonthsRequired * DaysPerMonth;

    public static bool MayPick(ClubGiftDefinition gift, Standing standing, ClubMembership membership) =>
        HasTime(gift, standing)
        && (gift.Vip ? membership == ClubMembership.Vip && VipPicks(standing) >= 1 : ClubPicks(standing) >= 1);
}
