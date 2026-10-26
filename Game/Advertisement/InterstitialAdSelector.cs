using DragonEmu.Domain;

namespace DragonEmu.Game.Advertisement;

public static class InterstitialAdSelector
{
    public static InterstitialAd? SelectAd(IReadOnlyList<InterstitialAd> ads, DateTime utcNow, Random random)
    {
        InterstitialAd[] active = ads.Where(ad => ad.IsActiveAt(utcNow)).ToArray();
        return active.Length == 0 ? null : active[random.Next(active.Length)];
    }
}
