using System.Collections.Concurrent;
using System.Globalization;
using DragonEmu.Game.Settings;
using DragonEmu.Domain;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.Pets;
using DragonEmu.Gateway;
using DragonEmu.Protocol.Messages.Outgoing.RoomAction;
using DragonEmu.Protocol.Messages.Outgoing.RoomChat;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Protocol.Messages.Outgoing.RoomFurniture;
using DragonEmu.Protocol.Messages.Outgoing.RoomPublicRoom;
using DragonEmu.Protocol.Messages.Outgoing.RoomSession;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Rooms;

public sealed class RoomMovementScheduler : IHostedService, IDisposable
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(500);

    private static readonly TimeSpan SubTickInterval = TimeSpan.FromMilliseconds(25);
    private static readonly int SubTicksPerTick = (int)(TickInterval / SubTickInterval);

    private static readonly TimeSpan TeleportArrivalWindow = TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<int, Room> _activeRooms = new();

    private readonly ConcurrentDictionary<int, Room> _footballRooms = new();
    private int _subTick;
    private readonly RoomBroadcaster _broadcaster;
    private readonly IRoomManager _roomManager;
    private readonly ClientSessionManager _sessionManager;
    private readonly GameSettingsCache _gameSettings;
    private readonly FurniturePositionWriter _positionWriter;
    private readonly TotemService _totems;
    private readonly PetService _pets;
    private readonly IdolService _idol;
    private readonly PublicRoomTransfers _transfers;
    private readonly FootballService _football;
    private readonly BanzaiService _banzai;
    private readonly FurnitureAutoStateService _autoStateService;
    private readonly ILogger<RoomMovementScheduler> _logger;
    private readonly PeriodicTimer _timer = new(SubTickInterval);
    private CancellationTokenSource? _stoppingCts;
    private Task? _loop;

    public RoomMovementScheduler(
        RoomBroadcaster broadcaster, IRoomManager roomManager, ClientSessionManager sessionManager,
        GameSettingsCache gameSettings, FurniturePositionWriter positionWriter, TotemService totems, PetService pets,
        PublicRoomTransfers transfers, IdolService idol, FootballService football,
        FurnitureAutoStateService autoStateService, BanzaiService banzai, ILogger<RoomMovementScheduler> logger)
    {
        _banzai = banzai;
        _autoStateService = autoStateService;
        _football = football;
        _idol = idol;
        _transfers = transfers;
        _pets = pets;
        _broadcaster = broadcaster;
        _roomManager = roomManager;
        _sessionManager = sessionManager;
        _gameSettings = gameSettings;
        _positionWriter = positionWriter;
        _totems = totems;
        _logger = logger;

        _roomManager.RoomUnloaded += _broadcaster.Forget;
    }

    public void MarkActive(Room room) => _activeRooms.TryAdd(room.Id, room);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        WindowsTimerResolution.Begin();
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loop = RunAsync(_stoppingCts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _stoppingCts?.Cancel();
        WindowsTimerResolution.End();

        if (_loop is not null)
        {
            try
            {
                await _loop.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(cancellationToken))
            {
                long nowMs = Environment.TickCount64;
                bool roomTick = ++_subTick % SubTicksPerTick == 0;

                if (!roomTick)
                {
                    foreach ((int roomId, Room room) in _footballRooms)
                    {
                        TickFootball(room, nowMs, roomTick: false);
                        AnnounceStacking(room);
                        _broadcaster.FlushPending(room);
                        if (!room.BallRolling)
                        {
                            _footballRooms.TryRemove(roomId, out _);
                        }
                    }

                    continue;
                }

                foreach ((int roomId, Room room) in _activeRooms)
                {
                    bool stillNeedsMovementTicks = room.AdvanceMovement(out List<RoomActor> changed);
                    FootballTickResult football = TickFootball(room, nowMs, roomTick: true);
                    bool footballBusy = football.NeedsFurtherTicks;
                    bool banzaiBusy = TickBanzai(room, football.Finished);
                    TeleporterTickResult teleporters = room.AdvanceTeleporters(changed);
                    List<OneWayDoorStatusChange> oneWayDoors = room.AdvanceOneWayDoors(changed, out bool oneWayDoorsBusy);
                    List<FurnitureStateChange> rolls = room.AdvanceRolls(out bool stillNeedsRollTicks);
                    RollerTickResult rollers = room.AdvanceRollers(changed, _gameSettings.Current.QueuetileFurniFusionEnabled);
                    VendingTickResult vending = room.AdvanceVending(changed);
                    List<FurnitureStateChange> hockeyLights = room.AdvanceHockeyLights(out bool hockeyLightsLit);
                    List<RoomActor> handItemsPutDown = room.AdvanceHandItems(out bool someoneHoldsAHandItem);
                    BotTickResult bots = AdvanceBots(room, changed);
                    PetTickResult pets = AdvancePets(room, changed);
                    IdolTickResult idol = room.AdvanceIdol(DateTime.UtcNow, changed);
                    changed.AddRange(room.AdvanceHeads(out bool headsTurned).Except(changed));

                    IReadOnlyList<AutoStateChange> autoStates = room.AdvanceAutoStates(changed);
                    if (autoStates.Count > 0)
                    {
                        _autoStateService.Apply(room, autoStates);
                    }

                    foreach (RoomActor wet in room.AdvanceWaterAreas())
                    {
                        int effect;
                        lock (wet.Lock)
                        {
                            effect = wet.EffectId;
                        }

                        _broadcaster.Broadcast(room, new AvatarEffectMessageEvent(wet.VirtualId, effect).Compose());
                    }

                    foreach (RoomActor dressed in room.TakeFigureChanges())
                    {
                        (string figure, char gender, string motto) = (string.Empty, 'M', string.Empty);
                        lock (dressed.Lock)
                        {
                            (figure, gender, motto) = (dressed.Figure, dressed.Gender, dressed.Motto);
                        }

                        _broadcaster.Broadcast(room, new UserChangeMessageEvent(dressed.VirtualId, figure, gender, motto).Compose());
                    }

                    foreach (FurnitureStateChange change in vending.StateChanges)
                    {
                        _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(change.ItemId, change.ExtraData).Compose());
                    }

                    foreach (OneWayDoorStatusChange change in oneWayDoors)
                    {
                        _broadcaster.Broadcast(room, new OneWayDoorStatusMessageEvent(change.ItemId, change.Status).Compose());
                    }

                    foreach (FurnitureStateChange change in hockeyLights)
                    {
                        _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(change.ItemId, change.ExtraData).Compose());
                    }

                    foreach (VendedHandItem vended in vending.HandItems)
                    {
                        _broadcaster.Broadcast(room, new CarryObjectMessageEvent(vended.Actor.VirtualId, vended.HandItemId).Compose());
                    }

                    foreach (RoomActor holder in handItemsPutDown)
                    {
                        _broadcaster.Broadcast(room, new CarryObjectMessageEvent(holder.VirtualId, 0).Compose());
                    }

                    foreach (TeleporterStateChange change in teleporters.StateChanges)
                    {
                        _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(
                            change.ItemId, change.State.ToString(CultureInfo.InvariantCulture)).Compose());
                    }

                    foreach (FurnitureStateChange roll in rolls)
                    {
                        _broadcaster.Broadcast(room, room.SetWallItemExtraData(roll.ItemId, roll.ExtraData) is { } wallItem
                            ? new ItemUpdateMessageEvent(wallItem).Compose()
                            : new ObjectDataUpdateMessageEvent(roll.ItemId, roll.ExtraData).Compose());
                    }

                    foreach (RollerSlide slide in rollers.Slides)
                    {
                        _broadcaster.Broadcast(room, new SlideObjectBundleMessageEvent(slide).Compose());
                    }

                    var restingRiders = new RestingRiderSlideMessageEvent(rollers.Slides);
                    if (!restingRiders.IsEmpty)
                    {
                        _broadcaster.Broadcast(room, restingRiders.Compose());
                    }

                    _positionWriter.Save(room, room.TakeUnsavedFloorPositions());

                    if (rollers.Slides.Any(slide => slide.Items.Any(item => room.GetFloorItem(item.ItemId) is { IsTotemPiece: true })))
                    {
                        _totems.SettleSoon(room);
                    }

                    if (changed.Count > 0)
                    {
                        _broadcaster.Broadcast(room, new UserUpdateMessageEvent(changed).Compose());
                    }

                    AnnounceBots(room, bots);
                    _pets.HandleTick(room, pets);
                    _idol.HandleTick(room, idol);

                    foreach (TeleporterDeparture departure in teleporters.Departures)
                    {
                        ForwardToPairRoom(departure);
                    }

                    foreach (RoomActor leaver in room.TakeActorsWalkedOut())
                    {
                        WalkOut(room, leaver, throughTheDoor: true);
                    }

                    foreach (RoomActor bather in room.TakeActorsInLockerRoom())
                    {
                        if (_sessionManager.TryGet(bather.SessionId, out ClientSession? changing))
                        {
                            changing!.EnqueueSend(new OpenLockerRoomMessageEvent().Compose());
                        }
                    }

                    foreach (RoomActor reached in room.TakeActorsAtBusTile())
                    {
                        if (_sessionManager.TryGet(reached.SessionId, out ClientSession? boarding) && boarding!.CurrentRoomId == room.Id)
                        {
                            _transfers.TryBoard(boarding);
                        }
                    }

                    foreach (WarpedActor warped in room.TakeActorsWarped())
                    {
                        if (_sessionManager.TryGet(warped.Actor.SessionId, out ClientSession? warping) && warping!.CurrentRoomId == room.Id)
                        {
                            _transfers.Warp(warping, warped.Warp, warped.Rotation);
                        }
                    }

                    List<RoomActor> kickedOut = room.AdvanceKicks(out bool kicksUnderWay);
                    foreach (RoomActor kicked in kickedOut)
                    {
                        WalkOut(room, kicked, throughTheDoor: false);
                    }

                    RoomQueueTickResult queue = room.AdvanceQueue(room.Data.MaxUsers, _gameSettings.Current.MaxSpectators, DateTime.UtcNow);
                    LetInTheQueue(room, queue);

                    AnnounceStacking(room);
                    _broadcaster.FlushPending(room);

                    if (!stillNeedsMovementTicks && !teleporters.NeedsFurtherTicks && !stillNeedsRollTicks && !rollers.NeedsFurtherTicks
                        && !someoneHoldsAHandItem && !vending.NeedsFurtherTicks && !hockeyLightsLit && !footballBusy && !banzaiBusy
                        && !oneWayDoorsBusy && !kicksUnderWay && !room.BotsAwake && !room.PetsAwake && !room.IdolBusy && !headsTurned
                        && !queue.NeedsFurtherTicks)
                    {
                        _activeRooms.TryRemove(roomId, out _);
                    }
                }

                foreach (int roomId in _broadcaster.PendingRoomIds)
                {
                    if (_activeRooms.ContainsKey(roomId))
                    {
                        continue;
                    }

                    if (_roomManager.TryGetLoadedRoom(roomId, out Room? room))
                    {
                        AnnounceStacking(room!);
                        _broadcaster.FlushPending(room!);

                        if (room!.RollersHaveWork() || room.BotsAwake || room.PetsAwake || room.HeadsTurned || room.FootballBusy || room.WaterRecheckPending || room.AutoStatesPending || room.BanzaiBusy)
                        {
                            MarkActive(room);
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RoomMovementScheduler tick loop crashed.");
        }
    }

    private void LetInTheQueue(Room room, RoomQueueTickResult queue)
    {
        foreach ((long sessionId, RoomQueueLine line) in queue.Admitted)
        {
            if (!_sessionManager.TryGet(sessionId, out ClientSession? session))
            {
                room.LeaveQueue(sessionId);
                continue;
            }

            session!.QueuedAt = null;
            session.Admission = new PendingRoomAdmission(room.Id, line, DateTime.UtcNow + Room.AdmissionWindow);
            session.EnqueueSend(new RoomForwardMessageEvent(isPublic: room.Data.OwnerId is null, room.Id).Compose());
        }

        bool spectatorsAllowed = RoomQueueService.SpectatorsAllowed(_gameSettings.Current);
        foreach ((long sessionId, RoomQueueStatus status) in queue.Moved)
        {
            if (_sessionManager.TryGet(sessionId, out ClientSession? waiting))
            {
                waiting!.EnqueueSend(new RoomQueueStatusMessageEvent(status, spectatorsAllowed).Compose());
            }
        }
    }

    private void ForwardToPairRoom(TeleporterDeparture departure)
    {
        if (!_sessionManager.TryGet(departure.SessionId, out ClientSession? session))
        {
            return;
        }

        session!.PendingTeleport = new PendingTeleportArrival(
            departure.RoomId, departure.TeleporterItemId, DateTime.UtcNow + TeleportArrivalWindow);
        session.EnqueueSend(new RoomForwardMessageEvent(isPublic: false, departure.RoomId).Compose());
    }

    private void WalkOut(Room room, RoomActor leaver, bool throughTheDoor)
    {
        if (_sessionManager.TryGet(leaver.SessionId, out ClientSession? session) && session!.CurrentRoomId == room.Id)
        {
            if (throughTheDoor && _transfers.IsInfobus(room))
            {
                _transfers.SendToPark(session);
                return;
            }

            RoomExit.ToHotelView(session, room, _broadcaster);
        }
        else if (room.TryRemoveActor(leaver.SessionId, out _))
        {
            _broadcaster.Broadcast(room, new UserRemoveMessageEvent(leaver.VirtualId).Compose());
        }
    }

    private BotTickResult AdvanceBots(Room room, List<RoomActor> changed)
    {
        try
        {
            return room.AdvanceBots(changed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bots in room {RoomId} failed this tick.", room.Id);
            return BotTickResult.None;
        }
    }

    private PetTickResult AdvancePets(Room room, List<RoomActor> changed)
    {
        try
        {
            return room.AdvancePets(changed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pets in room {RoomId} failed this tick.", room.Id);
            return PetTickResult.None;
        }
    }

    private void AnnounceBots(Room room, BotTickResult bots)
    {
        foreach (BotUtterance said in bots.Utterances)
        {
            switch (said.Kind)
            {
                case BotStepType.Shout:
                    _broadcaster.BroadcastLocalized(room, language => new ShoutMessageEvent(said.Bot.VirtualId, said.Said.For(language)).Compose());
                    break;
                case BotStepType.Whisper:
                    if (said.ToSessionId is { } to && _sessionManager.TryGet(to, out ClientSession? listener))
                    {
                        listener!.EnqueueSend(new WhisperMessageEvent(said.Bot.VirtualId, said.Said.For(listener.Language)).Compose());
                    }

                    break;
                default:
                    _broadcaster.BroadcastLocalized(room, language => new ChatMessageEvent(said.Bot.VirtualId, said.Said.For(language)).Compose());
                    break;
            }
        }

        foreach ((RoomActor holder, int itemId) in bots.HandItems)
        {
            _broadcaster.Broadcast(room, new CarryObjectMessageEvent(holder.VirtualId, itemId).Compose());
        }

        foreach (RoomActor waver in bots.Waves)
        {
            _broadcaster.Broadcast(room, new WaveMessageEvent(waver.VirtualId).Compose());
        }

        foreach ((RoomActor dancer, int danceId) in bots.Dances)
        {
            _broadcaster.Broadcast(room, new DanceMessageEvent(dancer.VirtualId, danceId).Compose());
        }
    }

    private FootballTickResult TickFootball(Room room, long nowMs, bool roomTick)
    {
        try
        {
            FootballTickResult football = room.AdvanceFootball(nowMs, roomTick);
            IReadOnlyList<BallMove> nudges = roomTick ? room.TakeBallNudges() : [];
            if (football.Moves.Count > 0 || football.Goals.Count > 0 || football.StateChanges.Count > 0 || football.Rests.Count > 0
                || football.Finished.Count > 0 || nudges.Count > 0)
            {
                _football.HandleTick(room, football, nudges);
            }

            if (room.BallRolling)
            {
                _footballRooms.TryAdd(room.Id, room);
            }

            return football;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The football in room {RoomId} failed this tick.", room.Id);
            return FootballTickResult.Idle;
        }
    }

    private bool TickBanzai(Room room, IReadOnlyList<CountdownFinished> finishedClocks)
    {
        try
        {
            BanzaiTickResult banzai = room.AdvanceBanzai(finishedClocks);
            if (banzai.Changes.Count > 0)
            {
                _banzai.HandleTick(room, banzai);
            }

            return banzai.Busy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Battle Banzai in room {RoomId} failed this tick.", room.Id);
            return false;
        }
    }

    private void AnnounceStacking(Room room)
    {
        if (room.TakeStackingMapUpdate() is { } update)
        {
            _broadcaster.Broadcast(room, new HeightMapUpdateMessageEvent(update).Compose());
        }
    }

    public void Dispose() => _timer.Dispose();
}
