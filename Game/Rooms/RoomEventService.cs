using System.Globalization;
using DragonEmu.Domain;
using DragonEmu.Game.Chat;
using DragonEmu.Game.Settings;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Game.Rooms;

public sealed class RoomEventService
{
    public const int NotInARoom = 1;
    public const int NotTheirsToHost = 2;
    public const int DoorNotOpen = 3;
    public const int EventsOff = 4;
    public const int AlreadyHasAnEvent = 5;

    public const int MaxNameLength = 25;
    public const int MaxDescriptionLength = 100;
    public const int MaxTagLength = 25;
    public const int MaxTags = 2;

    private const int FirstCategory = 1;
    private const int LastCategory = 11;
    private const int MaxListed = 50;
    private const string CancelEventFuse = "fuse_cancel_roomevent";

    private readonly IRoomManager _roomManager;
    private readonly RoomRightsService _roomRights;
    private readonly GameSettingsCache _gameSettings;
    private readonly ProfanityFilterCache _profanityFilter;
    private readonly HotelClock _clock;
    private readonly RoomBroadcaster _broadcaster;

    public RoomEventService(
        IRoomManager roomManager, RoomRightsService roomRights, GameSettingsCache gameSettings, ProfanityFilterCache profanityFilter,
        HotelClock clock, RoomBroadcaster broadcaster)
    {
        _roomManager = roomManager;
        _roomRights = roomRights;
        _gameSettings = gameSettings;
        _profanityFilter = profanityFilter;
        _clock = clock;
        _broadcaster = broadcaster;
    }

    public async Task SendCanCreateAsync(IGameSession session, UserAccount account, CancellationToken cancellationToken)
    {
        (int refusal, _) = await RoomToHostInAsync(session, account, cancellationToken);
        session.Send(new CanCreateRoomEventEvent(refusal).Compose());
    }

    public async Task CreateAsync(
        IGameSession session, UserAccount account, int category, string name, string description, IReadOnlyList<string> tags,
        CancellationToken cancellationToken)
    {
        if (await RoomToHostInAsync(session, account, cancellationToken) is not (0, { } room)
            || Text(name, MaxNameLength) is not { } eventName || Text(description, MaxDescriptionLength) is not { } eventDescription
            || !session.TryUseCooldown(ActionCooldowns.RoomEvent, ActionCooldowns.Default))
        {
            return;
        }

        string startTime = (_clock.TryGetNow(out DateTime now) ? now : DateTime.UtcNow).ToString("HH:mm", CultureInfo.InvariantCulture);
        var roomEvent = new RoomEvent(
            account.Id, account.Username, session.Id, category is >= FirstCategory and <= LastCategory ? category : FirstCategory,
            eventName, eventDescription, startTime, Tags(tags));
        if (room.TryStartEvent(roomEvent))
        {
            room.EventEnded -= AnnounceEnded;
            room.EventEnded += AnnounceEnded;
            _broadcaster.Broadcast(room, new RoomEventEvent(room.Id, roomEvent).Compose());
        }
    }

    public async Task EditAsync(
        IGameSession session, UserAccount account, string name, string description, IReadOnlyList<string> tags, CancellationToken cancellationToken)
    {
        if (CurrentEvent(session) is not ({ } room, { } current) || !await MayRunAsync(account, room, current, cancellationToken)
            || Text(name, MaxNameLength) is not { } eventName || Text(description, MaxDescriptionLength) is not { } eventDescription
            || !session.TryUseCooldown(ActionCooldowns.RoomEvent, ActionCooldowns.Default))
        {
            return;
        }

        RoomEvent updated = current with { Name = eventName, Description = eventDescription, Tags = Tags(tags) };
        if (room.TryReplaceEvent(current, updated))
        {
            _broadcaster.Broadcast(room, new RoomEventEvent(room.Id, updated).Compose());
        }
    }

    public async Task EndAsync(IGameSession session, UserAccount account, CancellationToken cancellationToken)
    {
        if (CurrentEvent(session) is ({ } room, { } current)
            && (await MayRunAsync(account, room, current, cancellationToken)
                || await _roomRights.HoldsFuseAsync(account, CancelEventFuse, cancellationToken)))
        {
            room.EndEvent();
        }
    }

    public void SendEvents(IGameSession session, string categoryParam)
    {
        int category = int.TryParse(categoryParam, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : 0;
        List<RoomListingEntry> rooms = [.. _roomManager.LoadedRooms
            .Select(room => (Room: room, Event: room.Event))
            .Where(entry => entry.Event is { } roomEvent && (category == 0 || roomEvent.Category == category))
            .Select(entry => new RoomListingEntry(entry.Room.Data, entry.Room.UserCount, entry.Event))
            .OrderByDescending(entry => entry.UsersNow)
            .ThenBy(entry => entry.Room.Id)
            .Take(MaxListed)];

        session.Send(new GuestRoomSearchResultEvent(GuestRoomSearchResultEvent.Events, categoryParam, rooms).Compose());
    }

    private async Task<(int Refusal, Room? Room)> RoomToHostInAsync(IGameSession session, UserAccount account, CancellationToken cancellationToken)
    {
        if (!_gameSettings.Current.RoomEventsEnabled)
        {
            return (EventsOff, null);
        }

        if (session.CurrentRoomId is not { } roomId || !_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || !room!.TryGetActor(session.Id, out _))
        {
            return (NotInARoom, null);
        }

        if (room.Data.OwnerId is null
            || (!RoomRightsService.IsOwner(account, room.Data) && !await _roomRights.HoldsAnyRoomControllerFuseAsync(account, cancellationToken)))
        {
            return (NotTheirsToHost, null);
        }

        return room.Data.AccessType != RoomAccess.Open ? (DoorNotOpen, null)
            : room.Event is not null ? (AlreadyHasAnEvent, null)
            : (0, room);
    }

    private (Room? Room, RoomEvent? Event) CurrentEvent(IGameSession session) =>
        session.CurrentRoomId is { } roomId && _roomManager.TryGetLoadedRoom(roomId, out Room? room) && room!.TryGetActor(session.Id, out _)
            ? (room, room.Event)
            : (null, null);

    private async Task<bool> MayRunAsync(UserAccount account, Room room, RoomEvent roomEvent, CancellationToken cancellationToken) =>
        roomEvent.HostUserId == account.Id || RoomRightsService.IsOwner(account, room.Data)
        || await _roomRights.HoldsAnyRoomControllerFuseAsync(account, cancellationToken);

    private string? Text(string text, int maxLength)
    {
        string cleaned = RoomTextSanitizer.Sanitize(text.Trim(), maxLength);
        return cleaned.Length == 0 ? null : _profanityFilter.Filter(cleaned);
    }

    private IReadOnlyList<string> Tags(IReadOnlyList<string> tags) =>
        [.. tags.Select(tag => Text(tag, MaxTagLength)).OfType<string>().Take(MaxTags)];

    private void AnnounceEnded(Room room) => _broadcaster.Broadcast(room, new RoomEventEvent(room.Id, null).Compose());
}
