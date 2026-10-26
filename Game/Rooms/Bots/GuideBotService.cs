using System.Collections.Concurrent;
using DragonEmu.Domain;
using DragonEmu.Game.Settings;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Handshake;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Rooms;

public sealed class GuideBotService : IHostedService
{
    public const int AlreadyExistsError = 4009;

    public const int NotAvailableError = 4010;

    private const int GuideBotIdBase = 1_000_000_000;

    private readonly IGuideBotRepository _repository;
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;
    private readonly RoomMovementScheduler _scheduler;
    private readonly GameSettingsCache _gameSettings;
    private readonly RoomRightsService _rights;
    private readonly ILogger<GuideBotService> _logger;
    private readonly object _calls = new();

    private readonly ConcurrentDictionary<int, int> _roomByOwner = new();
    private volatile GuideBotRecord? _record;

    public GuideBotService(
        IGuideBotRepository repository, IRoomManager roomManager, RoomBroadcaster broadcaster, RoomMovementScheduler scheduler,
        GameSettingsCache gameSettings, RoomRightsService rights, ILogger<GuideBotService> logger)
    {
        _repository = repository;
        _roomManager = roomManager;
        _broadcaster = broadcaster;
        _scheduler = scheduler;
        _gameSettings = gameSettings;
        _rights = rights;
        _logger = logger;
    }

    public int ActiveCount => _roomByOwner.Count;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _roomManager.RoomRemoved += ReleaseRoom;
        await ReloadAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _roomManager.RoomRemoved -= ReleaseRoom;
        return Task.CompletedTask;
    }

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        _record = await _repository.GetAsync(cancellationToken);
        if (_record is null)
        {
            _logger.LogWarning("Table guide_bot_data has no row. Therefore, no bot can be called.");
        }
    }

    public void Call(IGameSession session, UserAccount account)
    {
        if (session.CurrentRoomId is not { } roomId || !_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || !room!.TryGetActor(session.Id, out _) || room.Data.OwnerId != account.Id)
        {
            return;
        }

        RoomActor? guide;
        lock (_calls)
        {
            if (room.GuideBot is not null)
            {
                session.Send(new GenericErrorEvent(AlreadyExistsError).Compose());
                return;
            }

            if (_roomByOwner.ContainsKey(account.Id) || _roomByOwner.Count >= _gameSettings.Current.MaxGuideBotsInHotelAtOnce
                || Definition(account.Id) is not { } definition)
            {
                session.Send(new GenericErrorEvent(NotAvailableError).Compose());
                return;
            }

            guide = room.AddGuideBot(definition);
            if (guide is null)
            {
                session.Send(new GenericErrorEvent(AlreadyExistsError).Compose());
                return;
            }

            _roomByOwner[account.Id] = room.Id;
        }

        _logger.LogDebug("{Username} called a guide bot to room {RoomId}.", account.Username, room.Id);
        _broadcaster.Broadcast(room, new UsersMessageEvent([guide]).Compose());
        _broadcaster.Broadcast(room, new UserUpdateMessageEvent([guide]).Compose());
        _scheduler.MarkActive(room);
    }

    public async Task KickAsync(IGameSession session, UserAccount account, int virtualId, CancellationToken cancellationToken)
    {
        if (session.CurrentRoomId is not { } roomId || !_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || !await _rights.CanToggleFurnitureAsync(account, room!.Data, cancellationToken)
            || room.RemoveGuideBot(virtualId) is not { } gone)
        {
            return;
        }

        _broadcaster.Broadcast(room, new UserRemoveMessageEvent(gone.VirtualId).Compose());
        ReleaseRoom(room);
        _logger.LogDebug("{Username} kicked the guide bot out of room {RoomId}.", account.Username, room.Id);
    }

    private BotDefinition? Definition(int ownerId)
    {
        if (_record is not { } record)
        {
            return null;
        }

        var asRoomBot = new RoomBotRecord(
            GuideBotIdBase + ownerId, RoomId: 0, record.Name, record.Gender, record.Figure, record.Motto, X: 0, Y: 0, Z: 0, Rotation: 2,
            WalkableAreaJson: null, DiagonalMoveAllowed: true, record.Actions);
        return BotDefinitionParser.Parse(asRoomBot, warning => _logger.LogWarning("Guide bot: {Warning}", warning));
    }

    private void ReleaseRoom(Room room)
    {
        foreach ((int ownerId, int roomId) in _roomByOwner)
        {
            if (roomId == room.Id)
            {
                _roomByOwner.TryRemove(new KeyValuePair<int, int>(ownerId, roomId));
            }
        }
    }
}
