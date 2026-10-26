using System.Globalization;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Settings;

public sealed class HotelClock
{
    private readonly GameSettingsCache _gameSettings;
    private readonly ILogger<HotelClock> _logger;
    private Resolved _resolved = new(string.Empty, null);

    public HotelClock(GameSettingsCache gameSettings, ILogger<HotelClock> logger)
    {
        _gameSettings = gameSettings;
        _logger = logger;
    }

    public bool TryGetNow(out DateTime localNow)
    {
        TimeZoneInfo? zone = Resolve(_gameSettings.Current.HappyHour.Timezone);
        localNow = zone is null ? default : TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone);
        return zone is not null;
    }

    public DateTime ToHotelTime(DateTime utc) =>
        Resolve(_gameSettings.Current.HappyHour.Timezone) is { } zone
            ? TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone)
            : utc;

    public DateOnly Today => DateOnly.FromDateTime(TryGetNow(out DateTime localNow) ? localNow : DateTime.UtcNow);

    public string TodayText => Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public (DateTime Today, DateTime Yesterday) DayStartsUtc() =>
        DayStartsUtc(DateTime.UtcNow, Resolve(_gameSettings.Current.HappyHour.Timezone));

    public static (DateTime Today, DateTime Yesterday) DayStartsUtc(DateTime utcNow, TimeZoneInfo? zone)
    {
        if (zone is null)
        {
            return (utcNow.Date, utcNow.Date.AddDays(-1));
        }

        DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), zone).Date;
        return (StartUtc(today, zone), StartUtc(today.AddDays(-1), zone));
    }

    private static DateTime StartUtc(DateTime localMidnight, TimeZoneInfo zone)
    {
        DateTime local = DateTime.SpecifyKind(localMidnight, DateTimeKind.Unspecified);
        while (zone.IsInvalidTime(local))
        {
            local = local.AddMinutes(15);
        }

        return TimeZoneInfo.ConvertTimeToUtc(local, zone);
    }

    private TimeZoneInfo? Resolve(string timezoneId)
    {
        Resolved resolved = _resolved;
        if (resolved.Id == timezoneId)
        {
            return resolved.Zone;
        }

        TimeZoneInfo? zone = null;
        try
        {
            zone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            _logger.LogWarning(ex, "Can't resolve the hotel timezone {TimezoneId}.", timezoneId);
        }

        _resolved = new Resolved(timezoneId, zone);
        return zone;
    }

    private sealed record Resolved(string Id, TimeZoneInfo? Zone);
}
