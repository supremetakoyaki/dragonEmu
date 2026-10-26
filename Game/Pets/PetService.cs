using System.Globalization;
using System.Threading.Channels;
using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Fuses;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Rooms.Pathfinding;
using DragonEmu.Game.Settings;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Incoming.RoomEngine.AuxiliaryFunctions;
using DragonEmu.Protocol.Messages.Outgoing.Pets;
using DragonEmu.Protocol.Messages.Outgoing.RoomChat;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Pets;

public sealed class PetService : IHostedService, IDisposable
{
    private const string RespectGiverAchievement = "ACH_PetRespectGiver";
    private const string RespectReceiverAchievement = "ACH_PetRespectReceiver";
    private const string LevelUpAchievement = "ACH_PetLevelUp";
    private const string LoverAchievement = "ACH_PetLover";
    private const string FeedingAchievement = "ACH_PetFeeding";

    private static readonly TimeSpan SaveInterval = TimeSpan.FromSeconds(30);

    private readonly IPetRepository _pets;
    private readonly IUserItemRepository _userItems;
    private readonly PetDefinitionCache _definitions;
    private readonly PetNameRules _names;
    private readonly GameSettingsCache _gameSettings;
    private readonly HotelClock _clock;
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;
    private readonly ClientSessionManager _sessions;
    private readonly AchievementService _achievements;
    private readonly FuseService _fuses;
    private readonly ILogger<PetService> _logger;

    private readonly Channel<Func<CancellationToken, Task>> _work =
        Channel.CreateUnbounded<Func<CancellationToken, Task>>(new UnboundedChannelOptions { SingleReader = true });

    private readonly PeriodicTimer _saveTimer = new(SaveInterval);
    private CancellationTokenSource? _stopping;
    private Task? _workLoop;
    private Task? _saveLoop;

    public PetService(
        IPetRepository pets, IUserItemRepository userItems, PetDefinitionCache definitions, PetNameRules names, GameSettingsCache gameSettings,
        HotelClock clock, IRoomManager roomManager, RoomBroadcaster broadcaster, ClientSessionManager sessions, AchievementService achievements,
        FuseService fuses, ILogger<PetService> logger)
    {
        _pets = pets;
        _userItems = userItems;
        _definitions = definitions;
        _names = names;
        _gameSettings = gameSettings;
        _clock = clock;
        _roomManager = roomManager;
        _broadcaster = broadcaster;
        _sessions = sessions;
        _achievements = achievements;
        _fuses = fuses;
        _logger = logger;

        _roomManager.RoomLoaded += room => room.UserLeft += SendGuestPetsHomeWith;
        _roomManager.RoomRemoved += room => Enqueue(cancellationToken => SaveAsync([room], cancellationToken));
    }

    public PetNameCheck CheckName(string name) => _names.Check(PetNameRules.Normalize(name));

    public PetPurchase? TryReadPurchase(int type, string extraData)
    {
        PetDefinitions definitions = _definitions.Current;
        if (!_gameSettings.Current.Pets.Enabled
            || PetPurchase.Parse(type, extraData) is not { } purchase
            || _names.Check(purchase.Name) != PetNameCheck.Ok
            || !definitions.IsValidBreed(type, purchase.Breed)
            || !definitions.IsValidColor(type, purchase.Color))
        {
            return null;
        }

        return purchase;
    }

    public async Task GiveAsync(IGameSession session, UserAccount buyer, PetPurchase purchase, CancellationToken cancellationToken)
    {
        PetLevel first = _definitions.Current.LevelOf(1);
        int petId = await _pets.CreateAsync(
            buyer.Id, purchase.Type, purchase.Breed, purchase.Color, purchase.Name, first.EnergyCap, first.NutritionCap, cancellationToken);
        var pet = new PetRecord(
            petId, buyer.Id, buyer.Username, purchase.Type, purchase.Breed, purchase.Color, purchase.Name, RoomId: null, X: 0, Y: 0, Z: 0,
            Respect: 0, Level: 1, Experience: 0, first.EnergyCap, first.NutritionCap, DateTime.UtcNow);

        session.Send(new PetReceivedMessageEvent(pet).Compose());
        session.Send(new PetAddedToInventoryEvent(pet).Compose());
        await _achievements.SetAsync(buyer.Id, LoverAchievement, await _pets.CountOwnedAsync(buyer.Id, cancellationToken), cancellationToken);
    }

    public async Task SendInventoryAsync(IGameSession session, UserAccount account, CancellationToken cancellationToken) =>
        session.Send(new PetInventoryEvent(await _pets.GetInventoryAsync(account.Id, cancellationToken)).Compose());

    public async Task PlaceAsync(IGameSession session, UserAccount account, int petId, int x, int y, CancellationToken cancellationToken)
    {
        GameSettings settings = _gameSettings.Current;
        if (!settings.Pets.Enabled)
        {
            session.Send(new PetPlacingErrorEvent(PetPlacingError.ForbiddenInHotel).Compose());
            return;
        }

        if (session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || !room!.TryGetActor(session.Id, out RoomActor? placer))
        {
            return;
        }

        bool isRoomOwner = RoomRightsService.IsOwner(account, room.Data);
        if (room.Data.OwnerId is null
            || (!isRoomOwner && !room.Data.AllowPets && !await _fuses.HoldsAsync(account, RoomRightsService.AnyRoomControllerFuse, room.Data, cancellationToken)))
        {
            session.Send(new PetPlacingErrorEvent(PetPlacingError.ForbiddenInRoom).Compose());
            return;
        }

        if (room.PetCount >= settings.RoomFurnitureLimits.MaxPets
            && !await _fuses.HoldsAsync(account, RoomRightsService.AdministratorAccessFuse, cancellationToken: cancellationToken))
        {
            session.Send(new PetPlacingErrorEvent(PetPlacingError.TooManyPets).Compose());
            return;
        }

        if (await _pets.GetByIdAsync(petId, cancellationToken) is not { RoomId: null } pet || pet.OwnerId != account.Id)
        {
            return;
        }

        int rotation;
        if (isRoomOwner)
        {
            if (!room.CanPlacePetAt(x, y))
            {
                session.Send(new PetPlacingErrorEvent(PetPlacingError.SelectedTileNotFree).Compose());
                return;
            }

            (int placerX, int placerY) = Position(placer!);
            rotation = (placerX, placerY) == (x, y) ? 2 : RotationHelper.Calculate(x, y, placerX, placerY);
        }
        else
        {
            ((x, y), rotation) = Room.InFrontOf(placer!);
            if (!room.CanPlacePetAt(x, y))
            {
                session.Send(new PetPlacingErrorEvent(PetPlacingError.NoFreeTileInFront).Compose());
                return;
            }
        }

        double z = room.SurfaceHeightAt(x, y);
        if (!await _pets.PlaceAsync(petId, account.Id, roomId, x, y, z, cancellationToken))
        {
            return;
        }

        session.Send(new PetRemovedFromInventoryEvent(petId).Compose());

        RoomActor? actor = room.AddPet(pet with { RoomId = roomId, X = x, Y = y, Z = z }, _definitions.Current, x, y, rotation);
        if (actor is null)
        {
            return;
        }

        _broadcaster.Broadcast(room, new UsersMessageEvent([actor]).Compose());
        _broadcaster.Broadcast(room, new UserUpdateMessageEvent([actor]).Compose());
    }

    public async Task PickUpAsync(IGameSession session, UserAccount account, int webId, CancellationToken cancellationToken)
    {
        if (PlacedPet(session, webId) is not ({ } room, { Pet: { } brain }))
        {
            return;
        }

        if (brain.OwnerId != account.Id
            && !RoomRightsService.IsOwner(account, room.Data)
            && !await _fuses.HoldsAsync(account, RoomRightsService.AnyRoomControllerFuse, room.Data, cancellationToken))
        {
            return;
        }

        SendHome(room, brain.PetId);
    }

    public void SendInfo(IGameSession session, int webId)
    {
        if (PlacedPet(session, webId) is not (_, { Pet: { } brain }))
        {
            return;
        }

        PetStats stats = brain.Stats;
        PetDefinitions definitions = brain.Definitions;
        session.Send(new PetInfoMessageEvent(
            webId, brain.Name, stats, definitions.MaxLevel, definitions.LevelOf(stats.Level), brain.Figure, brain.OwnerId,
            DaysSince(brain.CreatedAt), brain.OwnerName).Compose());
    }

    public void SendCommands(IGameSession session, int webId)
    {
        if (PlacedPet(session, webId) is (_, { Pet: { } brain }))
        {
            session.Send(new PetCommandsMessageEvent(webId, brain.Definitions.CommandsKnownAt(brain.Stats.Level)).Compose());
        }
    }

    public async Task RespectAsync(IGameSession session, UserAccount account, int webId, CancellationToken cancellationToken)
    {
        GameSettings settings = _gameSettings.Current;
        if (!settings.Pets.Enabled || PlacedPet(session, webId) is not ({ } room, { Pet: { } brain }))
        {
            return;
        }

        int accountAge = DaysSince(account.CreatedAt);
        if (accountAge < settings.Pets.RespectMinimumAccountDays)
        {
            session.Send(new PetRespectFailedEvent(settings.Pets.RespectMinimumAccountDays, accountAge).Compose());
            return;
        }

        if (!await _pets.TryUsePetRespectAsync(account.Id, _clock.Today, settings.DailyPetRespectGrant, cancellationToken)
            || await _pets.AddRespectAsync(brain.PetId, cancellationToken) is not { } respect)
        {
            return;
        }

        brain.Respected(respect);
        _broadcaster.Broadcast(room, new PetRespectNotificationEvent(respect, brain.OwnerId, webId, brain.Name, brain.Figure, brain.Type).Compose());

        await _achievements.IncrementAsync(account.Id, RespectGiverAchievement, cancellationToken: cancellationToken);
        await _achievements.IncrementAsync(brain.OwnerId, RespectReceiverAchievement, cancellationToken: cancellationToken);
    }

    public void SendGuestPetsHome(Room room)
    {
        foreach (RoomActor pet in room.Pets)
        {
            if (pet.Pet is { } brain && brain.OwnerId != room.Data.OwnerId)
            {
                SendHome(room, brain.PetId);
            }
        }
    }

    private void SendGuestPetsHomeWith(Room room, RoomActor user)
    {
        if (!room.HasPets || user.AccountId == room.Data.OwnerId)
        {
            return;
        }

        foreach (RoomActor pet in room.Pets)
        {
            if (pet.Pet is { } brain && brain.OwnerId == user.AccountId)
            {
                SendHome(room, brain.PetId);
            }
        }
    }

    private void SendHome(Room room, int petId)
    {
        if (room.RemovePet(petId) is not { Pet: { } brain } pet)
        {
            return;
        }

        _broadcaster.Broadcast(room, new UserRemoveMessageEvent(pet.VirtualId).Compose());
        PetRecord home = brain.AsInventoryRecord();
        Enqueue(async cancellationToken =>
        {
            if (await _pets.ReturnToInventoryAsync(petId, brain.Stats, cancellationToken)
                && _sessions.TryGetByUserId(home.OwnerId, out ClientSession? owner))
            {
                owner!.EnqueueSend(new PetAddedToInventoryEvent(home).Compose());
            }
        });
    }

    public string? BasalExtraData(FurnitureDefinition? definition) =>
        definition?.InteractionLogic == FurnitureInteractionLogic.PetWater && _definitions.Current.FoodFor(definition.Classname) is { } water
            ? water.FullState.ToString(CultureInfo.InvariantCulture)
            : null;

    public void TrackItem(Room room, RoomFloorItem item, string? extraData)
    {
        if (Room.IsPetItem(item.InteractionLogic))
        {
            room.TrackPetItem(item.Id, extraData, _definitions.Current.FoodFor(item.Classname)?.FullState ?? 0);
        }
    }

    public async Task RefillWaterAsync(Room room, RoomFloorItem bowl, CancellationToken cancellationToken)
    {
        if (_definitions.Current.FoodFor(bowl.Classname) is not { } water || !room.RefillPetWater(bowl.Id, water.FullState))
        {
            return;
        }

        string state = water.FullState.ToString(CultureInfo.InvariantCulture);
        await _userItems.UpdateExtraDataAsync(bowl.Id, state, cancellationToken);
        _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(bowl.Id, state).Compose());
    }

    public void HandleTick(Room room, PetTickResult result)
    {
        foreach (PetUtterance said in result.Utterances)
        {
            _broadcaster.Broadcast(room, new ChatMessageEvent(said.Pet.VirtualId, said.Text).Compose());
        }

        foreach (PetExperienceGain gain in result.Experience)
        {
            if (gain.Pet.Pet is not { } brain)
            {
                continue;
            }

            _broadcaster.Broadcast(room, new PetExperienceEvent(PetWebId.Of(brain.PetId), gain.Pet.VirtualId, gain.Amount).Compose());
            if (gain.LevelsReached.Count == 0)
            {
                continue;
            }

            if (_sessions.TryGetByUserId(brain.OwnerId, out ClientSession? owner))
            {
                foreach (int level in gain.LevelsReached)
                {
                    owner!.EnqueueSend(new PetLevelNotificationEvent(brain.PetId, brain.Name, level, brain.Figure).Compose());
                }

                owner!.EnqueueSend(new PetCommandsMessageEvent(
                    PetWebId.Of(brain.PetId), brain.Definitions.CommandsKnownAt(gain.LevelsReached[^1])).Compose());
            }

            int levels = gain.LevelsReached.Count;
            Enqueue(cancellationToken => _achievements.IncrementAsync(brain.OwnerId, LevelUpAchievement, levels, cancellationToken));
        }

        foreach (FurnitureStateChange change in result.ItemStates)
        {
            _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(change.ItemId, change.ExtraData).Compose());

            if (room.GetFloorItem(change.ItemId) is { InteractionLogic: not FurnitureInteractionLogic.PetToy })
            {
                Enqueue(cancellationToken => _userItems.UpdateExtraDataAsync(change.ItemId, change.ExtraData, cancellationToken));
            }
        }

        foreach (PetMeal meal in result.Meals)
        {
            if (meal.Pet.Pet is { } brain && meal.Nutrition > 0)
            {
                Enqueue(cancellationToken => _achievements.IncrementAsync(brain.OwnerId, FeedingAchievement, meal.Nutrition, cancellationToken));
            }

            if (meal.EatenUp)
            {
                FloorItemRemoval.RemoveFromRoom(room, meal.ItemId, _broadcaster);
                Enqueue(cancellationToken => _userItems.DeleteFromRoomAsync(meal.ItemId, room.Id, cancellationToken));
            }
        }
    }

    private async Task SaveAsync(IEnumerable<Room> rooms, CancellationToken cancellationToken)
    {
        var saves = new List<PetSave>();
        foreach (Room room in rooms)
        {
            foreach (RoomActor pet in room.Pets)
            {
                if (pet.Pet is { } brain && brain.TakeDirty())
                {
                    lock (pet.Lock)
                    {
                        saves.Add(new PetSave(brain.PetId, brain.Stats, pet.X, pet.Y, pet.Z));
                    }
                }
            }
        }

        if (saves.Count > 0)
        {
            await _pets.SaveAsync(saves, cancellationToken);
        }
    }

    private void Enqueue(Func<CancellationToken, Task> work) => _work.Writer.TryWrite(work);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _stopping = new CancellationTokenSource();
        _workLoop = Task.Run(WorkAsync, CancellationToken.None);
        _saveLoop = Task.Run(() => SaveEveryIntervalAsync(_stopping.Token), CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _stopping?.Cancel();
        if (_saveLoop is not null)
        {
            await _saveLoop;
        }

        Enqueue(token => SaveAsync(_roomManager.LoadedRooms, token));
        _work.Writer.TryComplete();
        if (_workLoop is not null)
        {
            try
            {
                await _workLoop.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Shutdown didn't wait for every pet to be saved.");
            }
        }
    }

    private async Task WorkAsync()
    {
        await foreach (Func<CancellationToken, Task> work in _work.Reader.ReadAllAsync())
        {
            try
            {
                await work(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saving something pets did failed.");
            }
        }
    }

    private async Task SaveEveryIntervalAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _saveTimer.WaitForNextTickAsync(cancellationToken))
            {
                Enqueue(token => SaveAsync(_roomManager.LoadedRooms, token));
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private (Room Room, RoomActor Pet)? PlacedPet(IGameSession session, int webId)
    {
        if (PetWebId.PetIdOf(webId) is { } petId
            && session.CurrentRoomId is { } roomId
            && _roomManager.TryGetLoadedRoom(roomId, out Room? room)
            && room!.GetPet(petId) is { } pet)
        {
            return (room, pet);
        }

        return null;
    }

    private static (int X, int Y) Position(RoomActor actor)
    {
        lock (actor.Lock)
        {
            return (actor.X, actor.Y);
        }
    }

    private static int DaysSince(DateTime utc) => Math.Max(0, (int)(DateTime.UtcNow - utc).TotalDays);

    public void Dispose()
    {
        _saveTimer.Dispose();
        _stopping?.Dispose();
    }
}
