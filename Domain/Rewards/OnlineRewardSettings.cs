namespace DragonEmu.Domain;

public sealed record OnlineRewardSettings(bool Enabled, int CreditInterval, int CreditAmount, int PixelInterval, int PixelAmount)
{
    public static readonly OnlineRewardSettings Default = new(true, 60, 25, 13, 5);

    public bool CreditsOn => Enabled && CreditInterval > 0 && CreditAmount > 0;

    public bool PixelsOn => Enabled && PixelInterval > 0 && PixelAmount > 0;
}
