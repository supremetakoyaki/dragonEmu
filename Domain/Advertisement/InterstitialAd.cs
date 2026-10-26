namespace DragonEmu.Domain;

public sealed record InterstitialAd(
    int Id,
    string ImageUrl,
    string Link,
    DateTime? ActiveFrom,
    DateTime? ActiveUntil)
{
    public bool IsActiveAt(DateTime utcNow) =>
        (ActiveFrom is not { } from || utcNow >= from) && (ActiveUntil is not { } until || utcNow <= until);
}
