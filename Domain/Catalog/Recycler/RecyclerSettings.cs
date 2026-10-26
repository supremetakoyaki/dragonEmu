namespace DragonEmu.Domain;

public sealed record RecyclerSettings(bool Open, int CooldownSeconds, int PrizeBoxFurniDefId)
{
    public int SecondsLeft(DateTime? lastRecycledAt, DateTime utcNow) =>
        lastRecycledAt is { } last ? (int)Math.Max(0, Math.Ceiling((last.AddSeconds(CooldownSeconds) - utcNow).TotalSeconds)) : 0;
}
