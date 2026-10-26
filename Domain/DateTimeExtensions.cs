namespace DragonEmu.Domain;

public static class DateTimeExtensions
{
    public static DateTime ToWholeSecond(this DateTime time) => time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond));
}
