namespace DragonEmu.Domain;

public sealed record AvatarEffect(int EffectId, int Duration, int Quantity, DateTime? ExpiresAt)
{
    public int? SecondsLeft(DateTime utcNow) =>
        ExpiresAt is { } expiresAt ? (int)Math.Max(0, Math.Ceiling((expiresAt - utcNow).TotalSeconds)) : null;
}
