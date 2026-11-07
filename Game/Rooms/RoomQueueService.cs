using DragonEmu.Domain;
using DragonEmu.Game.Settings;
using DragonEmu.Game.Subscriptions;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.RoomSession;

namespace DragonEmu.Game.Rooms;

public sealed class RoomQueueService
{
    private readonly IRoomManager _roomManager;
    private readonly RoomMovementScheduler _scheduler;
    private readonly SubscriptionManager _subscriptions;
    private readonly GameSettingsCache _settings;

    public RoomQueueService(IRoomManager roomManager, RoomMovementScheduler scheduler, SubscriptionManager subscriptions, GameSettingsCache settings)
    {
        _roomManager = roomManager;
        _scheduler = scheduler;
        _subscriptions = subscriptions;
        _settings = settings;
    }

    public static bool SpectatorsAllowed(GameSettings settings) => settings.MaxSpectators > 0;

    public async Task JoinAsync(IGameSession session, Room room, int userId, CancellationToken cancellationToken)
    {
        bool club = await _subscriptions.IsClubMemberAsync(userId, cancellationToken);
        session.QueuedAt = room.Id;
        RoomQueueStatus status = room.JoinQueue(session.Id, club, RoomQueueLine.Visitors);
        session.Send(new RoomQueueStatusMessageEvent(status, SpectatorsAllowed(_settings.Current)).Compose());
        _scheduler.MarkActive(room);
    }

    public void ChangeLine(IGameSession session, RoomQueueLine line)
    {
        bool spectatorsAllowed = SpectatorsAllowed(_settings.Current);
        if ((line == RoomQueueLine.Visitors || spectatorsAllowed)
            && session.QueuedAt is { } roomId && _roomManager.TryGetLoadedRoom(roomId, out Room? room)
            && room!.ChangeQueueLine(session.Id, line) is { } status)
        {
            session.Send(new RoomQueueStatusMessageEvent(status, spectatorsAllowed).Compose());
            _scheduler.MarkActive(room);
        }
    }

    public bool Leave(IGameSession session)
    {
        if (session.QueuedAt is not { } roomId)
        {
            return false;
        }

        session.QueuedAt = null;
        if (_roomManager.TryGetLoadedRoom(roomId, out Room? room))
        {
            room!.LeaveQueue(session.Id);
        }

        return true;
    }

    public static RoomQueueLine? TurnOf(IGameSession session, Room room) =>
        session.Admission is { } admission && admission.IsFor(room.Id, DateTime.UtcNow) ? admission.Line : null;

    public static void TakeTurn(IGameSession session, Room room)
    {
        session.Admission = null;
        room.TakeAdmission(session.Id);
    }
}
