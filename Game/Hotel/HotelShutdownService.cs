using DragonEmu.Gateway;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Settings;
using DragonEmu.Protocol.Messages.Outgoing.Availability;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Hotel;

public sealed class HotelShutdownService
{
    public static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(240 * 500);

    public static readonly TimeSpan DefaultReopenAfter = TimeSpan.FromMinutes(10);

    private static readonly TimeSpan FlushDelay = TimeSpan.FromSeconds(2);

    private readonly HotelStatus _status;
    private readonly ClientSessionManager _sessions;
    private readonly IRoomManager _roomManager;
    private readonly HotelClock _clock;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<HotelShutdownService> _logger;

    public HotelShutdownService(
        HotelStatus status, ClientSessionManager sessions, IRoomManager roomManager, HotelClock clock, IHostApplicationLifetime lifetime,
        ILogger<HotelShutdownService> logger)
    {
        _status = status;
        _sessions = sessions;
        _roomManager = roomManager;
        _clock = clock;
        _lifetime = lifetime;
        _logger = logger;
    }

    public static int MinutesLeft => (int)Math.Ceiling(Delay.TotalMinutes);

    public bool TryBegin(string startedBy, TimeOnly? reopensAt)
    {
        DateTime closesAt = _clock.ToHotelTime(DateTime.UtcNow + Delay);
        TimeOnly reopens = reopensAt ?? TimeOnly.FromDateTime(closesAt + DefaultReopenAfter);
        if (!_status.TryBegin(reopens))
        {
            return false;
        }

        _logger.LogWarning("{Username} began the hotel shutdown. It will close in {Seconds} and open again at {Reopens:HH:mm}.", startedBy, Delay.TotalSeconds, reopens);

        PauseRoomPets(true);
        _roomManager.RoomLoaded += room => room.PetActivityPaused = true;

        _sessions.BroadcastToAll(new AvailabilityStatusMessageEvent(open: true, onShutdown: true).Compose().ToArray());
        _sessions.BroadcastToAll(new InfoHotelClosingMessageEvent(MinutesLeft).Compose().ToArray());

        _ = CloseAfterAsync(reopens);
        return true;
    }

    private void PauseRoomPets(bool paused)
    {
        foreach (Room room in _roomManager.LoadedRooms)
        {
            room.PetActivityPaused = paused;
        }
    }

    private async Task CloseAfterAsync(TimeOnly reopens)
    {
        try
        {
            await Task.Delay(Delay);

            _sessions.BroadcastToAll(new InfoHotelClosedMessageEvent(reopens.Hour, reopens.Minute, userThrownOut: true).Compose().ToArray());
            await Task.Delay(FlushDelay);
            foreach (ClientSession session in _sessions.GetAuthenticatedSessions())
            {
                session.Close();
            }

            await Task.Delay(FlushDelay);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Closing the hotel failed; stopping anyway.");
        }

        _lifetime.StopApplication();
    }
}
