namespace DragonEmu.Domain;

public sealed record ClubOfferDefinition(int OfferId, int PeriodDays, bool Vip, bool Upgrade);

public sealed record ClubOffer(CatalogOffer Offer, int PeriodDays, bool Vip, bool Upgrade = false)
{
    public int OfferId => Offer.Id;

    public int Months => Math.Max(1, PeriodDays / ClubOfferRules.DaysPerMonth);
}

public enum ClubMembership
{
    None,
    HabboClub,
    Vip,
}

public sealed record ClubOfferListing(int OfferId, string Code, int Price, bool Upgrade, bool Vip, int Months, int DaysLeftAfterPurchase, DateTime EndsAt);

public static class ClubOfferRules
{
    public const int DaysPerMonth = 31;

    public static bool IsListedTo(ClubMembership membership, ClubOffer offer) => membership switch
    {
        ClubMembership.Vip => offer.Vip && !offer.Upgrade,
        ClubMembership.HabboClub => !offer.Vip || offer.Upgrade,
        _ => !offer.Upgrade,
    };

    public static bool MayBuy(ClubMembership membership, bool vipOffer, bool upgrade = false) => membership switch
    {
        ClubMembership.Vip => vipOffer && !upgrade,
        ClubMembership.HabboClub => true,
        _ => !upgrade,
    };

    public static DateTime EndAfter(DateTime? expiresAt, DateTime now, int days) =>
        (expiresAt is { } end && end > now ? end : now).AddDays(days);

    public static int DaysLeft(DateTime? expiresAt, DateTime now) =>
        expiresAt is { } end && end > now ? (int)Math.Ceiling((end - now).TotalDays) : 0;

    public static int? UpgradeDays(ClubOffer upgrade, IEnumerable<ClubOffer> offers, int daysLeft)
    {
        ClubOffer[] plain = [.. offers.Where(o => !o.Upgrade && o.PeriodDays > 0)];
        ClubOffer? club = plain.Where(o => !o.Vip).OrderBy(o => o.PeriodDays).FirstOrDefault();
        ClubOffer? vip = plain.Where(o => o.Vip).OrderBy(o => o.PeriodDays).FirstOrDefault();
        if (club is null || vip is null || vip.Offer.PriceCredits <= 0)
        {
            return null;
        }

        double clubPerDay = (double)club.Offer.PriceCredits / club.PeriodDays;
        double vipPerDay = (double)vip.Offer.PriceCredits / vip.PeriodDays;
        return (int)Math.Floor(daysLeft * clubPerDay / vipPerDay) + (int)Math.Floor(upgrade.Offer.PriceCredits / vipPerDay);
    }

    public static IReadOnlyList<ClubOfferListing> Listing(IReadOnlyList<ClubOffer> offers, ClubMembership membership, DateTime? expiresAt, DateTime now)
    {
        var listing = new List<ClubOfferListing>();
        foreach (ClubOffer o in offers.Where(o => IsListedTo(membership, o))
                     .OrderBy(o => o.Upgrade).ThenBy(o => o.PeriodDays).ThenBy(o => o.Vip).ThenBy(o => o.OfferId))
        {
            DateTime end;
            int days;
            if (o.Upgrade)
            {
                if (UpgradeDays(o, offers, DaysLeft(expiresAt, now)) is not { } upgraded)
                {
                    continue;
                }

                days = upgraded;
                end = now.AddDays(days);
            }
            else
            {
                end = EndAfter(expiresAt, now, o.PeriodDays);
                days = (int)Math.Ceiling((end - now).TotalDays);
            }

            listing.Add(new ClubOfferListing(o.OfferId, o.Offer.OfferCode, o.Offer.PriceCredits, o.Upgrade, o.Vip, o.Months, days, end));
        }

        return listing;
    }
}
