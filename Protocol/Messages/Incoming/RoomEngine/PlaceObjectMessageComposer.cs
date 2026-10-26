using DragonEmu.Game.Settings;
using DragonEmu.Domain;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.Pets;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Trading;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Incoming.RoomEngine.AuxiliaryFunctions;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine;

public sealed class PlaceObjectMessageComposer : IMessageComposer
{
    private const string FurniChooserFuse = "fuse_furni_chooser";

    private readonly IRoomManager _roomManager;
    private readonly IUserItemRepository _userItemRepository;
    private readonly FurnitureDefinitionCache _furnitureDefinitions;
    private readonly GameSettingsCache _gameSettings;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly RoomRightsService _roomRights;
    private readonly FurnitureRollService _rolls;
    private readonly RoomMovementScheduler _scheduler;
    private readonly TotemService _totems;
    private readonly TradingService _trading;
    private readonly PetService _pets;
    private readonly MessageTexts _texts;

    private string _body = string.Empty;

    public PlaceObjectMessageComposer(
        IRoomManager roomManager,
        IUserItemRepository userItemRepository,
        FurnitureDefinitionCache furnitureDefinitions,
        GameSettingsCache gameSettings,
        RoomBroadcaster roomBroadcaster,
        RoomRightsService roomRights,
        FurnitureRollService rolls,
        RoomMovementScheduler scheduler,
        TotemService totems,
        TradingService trading,
        PetService pets, MessageTexts texts)
    {
        _texts = texts;
        _pets = pets;
        _roomManager = roomManager;
        _userItemRepository = userItemRepository;
        _furnitureDefinitions = furnitureDefinitions;
        _gameSettings = gameSettings;
        _roomBroadcaster = roomBroadcaster;
        _roomRights = roomRights;
        _rolls = rolls;
        _scheduler = scheduler;
        _totems = totems;
        _trading = trading;
    }

    public void Parse(ClientPacket packet)
    {
        _body = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || session.CurrentRoomId is not { } roomId)
        {
            return;
        }

        string[] tokens = _body.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!_roomManager.TryGetLoadedRoom(roomId, out Room? room))
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.Generic).Compose());
            return;
        }

        if (tokens.Length >= 2 && int.TryParse(tokens[0], out int wallItemId) && tokens[1].StartsWith(':'))
        {
            await HandleWallPlacementAsync(session, room!, account, roomId, wallItemId, string.Join(' ', tokens[1..]), cancellationToken);
            return;
        }

        if (!await _roomRights.CanUseFurnitureAsync(account, room!.Data, cancellationToken))
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.Generic).Compose());
            return;
        }

        if (tokens.Length != 4
            || !int.TryParse(tokens[0], out int itemId)
            || !int.TryParse(tokens[1], out int x)
            || !int.TryParse(tokens[2], out int y)
            || !int.TryParse(tokens[3], out int rawDirection))
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.Generic).Compose());
            return;
        }

        UserItem? item = await _userItemRepository.GetByIdAsync(itemId, cancellationToken);
        if (item is null || item.Type != FurnitureType.Floor || item.RoomRef is not null || _trading.IsOffered(session, itemId))
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.ItemNotFound).Compose());
            return;
        }

        FurnitureDefinition? definition = _furnitureDefinitions.GetById(item.FurniDefId);
        FurnitureRollKind rollKind = RollKindOf(definition);

        if (definition is not null && Room.IsSoundFurni(definition.LogicClass) && room.SoundFurni is not null)
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.MaxSoundFurni).Compose());
            return;
        }

        bool isRoller = definition?.InteractionLogic == FurnitureInteractionLogic.Roller;
        if (await ExceededLimitAsync(account, room, isSticky: false, isRoller, cancellationToken) is { } limit)
        {
            session.Send(new PlaceObjectErrorMessageEvent(limit).Compose());
            return;
        }

        int rotation = (rawDirection % 8) * 45;

        (x, y) = room.PlacementTile(x, y);

        var placing = new RoomFloorItem(
            itemId, x, y, 0, rotation,
            definition?.XDim ?? 1, definition?.YDim ?? 1,
            definition?.IsWalkable ?? false, definition?.IsSittable ?? false, definition?.IsBed ?? false,
            definition?.IsStackable ?? false, definition?.ZDim ?? 0, definition?.Classname ?? "",
            definition?.InteractionLogic ?? FurnitureInteractionLogic.None, definition?.States,
            definition?.LogicClass ?? FurnitureLogicClass.Basic, definition?.VisualizationClass ?? FurnitureVisualizationClass.Static,
            definition?.InteractionIds);

        HashSet<(int X, int Y)> footprint = placing.OccupiedTiles().ToHashSet();
        HashSet<(int X, int Y)> layTiles = placing.IsBed ? placing.LayTiles().ToHashSet() : [];
        var exempt = new HashSet<long>();

        foreach (long sessionId in room.StandingActorsOn(footprint))
        {
            if (!room.TryGetActor(sessionId, out RoomActor? standingActor))
            {
                continue;
            }

            bool allowed;
            lock (standingActor!.Lock)
            {
                allowed = placing.IsSittable || (placing.IsBed && layTiles.Contains((standingActor.X, standingActor.Y)));
            }

            if (!allowed)
            {
                session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.NotEnoughSpace).Compose());
                return;
            }

            exempt.Add(sessionId);
        }

        foreach ((int fx, int fy) in footprint)
        {
            if (!room.Grid.CanHoldFurniture(fx, fy))
            {
                session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.NotEnoughSpace).Compose());
                return;
            }
        }

        FloorTileResolver.Result tile = FloorTileResolver.Resolve(
            room, x, y, placedItemIsWalkable: definition?.IsWalkable ?? false, excludeItemId: itemId, exempt);
        if (!tile.Success)
        {
            session.Send(new PlaceObjectErrorMessageEvent(tile.Error).Compose());
            return;
        }

        if (room.TryAddFloorItemOnStack(placing, exempt) is not { } z)
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.NotEnoughSpace).Compose());
            return;
        }

        bool placedAtZero = definition?.InteractionLogic is FurnitureInteractionLogic.Vending or FurnitureInteractionLogic.Gate
            or FurnitureInteractionLogic.HockeyLight || definition?.LogicClass == FurnitureLogicClass.OneWayDoor
            || (definition is not null && Room.IsSoundFurni(definition.LogicClass));
        string? basalExtraData = placedAtZero ? "0" : _pets.BasalExtraData(definition) ?? FurnitureRolls.BasalExtraData(rollKind);
        bool placed = await _userItemRepository.PlaceOnFloorAsync(itemId, account.Id, roomId, x, y, (decimal)z, rotation,
            basalExtraData, placedAtZero || FurnitureRolls.ResetsWhenPlaced(rollKind), cancellationToken);
        if (!placed)
        {
            room.RemoveFloorItem(itemId);
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.ItemNotFound).Compose());
            return;
        }

        room.ConfirmFloorItem(itemId);
        _scheduler.MarkActive(room);

        session.Send(new FurniListRemoveEvent(itemId).Compose());

        UserItem? placedItem = await _userItemRepository.GetByIdAsync(itemId, cancellationToken);
        if (placedItem is not null)
        {
            _roomBroadcaster.Broadcast(room, new ObjectAddMessageEvent(placedItem).Compose());

            _pets.TrackItem(room, placing, placedItem.ExtraData);
        }

        if (placing.IsTotemPiece)
        {
            await _totems.SettleAsync(room, cancellationToken);
        }

        var resettled = new List<RoomActor>();
        foreach (long sessionId in exempt)
        {
            if (room.TryGetActor(sessionId, out RoomActor? settledActor))
            {
                lock (settledActor!.Lock)
                {
                    if (room.ApplyTilePosture(settledActor))
                    {
                        resettled.Add(settledActor);
                    }
                }
            }
        }

        if (resettled.Count > 0)
        {
            _roomBroadcaster.Broadcast(room, new UserUpdateMessageEvent(resettled).Compose());
        }
    }

    private async ValueTask HandleWallPlacementAsync(
        IGameSession session, Room room, UserAccount account, int roomId, int itemId, string wallPositionRaw, CancellationToken cancellationToken)
    {
        if (!WallPositionParser.TryParse(wallPositionRaw, out int wallX, out int wallY, out int offsetX, out int offsetY, out string direction)
            || wallX < 0 || wallX >= room.Grid.Width
            || wallY < 0 || wallY >= room.Grid.Height)
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.NotEnoughSpace).Compose());
            return;
        }

        UserItem? item = await _userItemRepository.GetByIdAsync(itemId, cancellationToken);
        if (item is { Type: FurnitureType.Wall, LogicClass: FurnitureLogicClass.Stickie } && item.RoomRef == roomId)
        {
            await MoveStickieAsync(session, room, account, roomId, itemId,
                WallPositionParser.Format(wallX, wallY, offsetX, offsetY, direction), cancellationToken);
            return;
        }

        if (item is null || item.Type != FurnitureType.Wall || item.RoomRef is not null || _trading.IsOffered(session, itemId))
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.ItemNotFound).Compose());
            return;
        }

        bool isStickie = item.LogicClass == FurnitureLogicClass.Stickie;
        bool mayPlace = isStickie
            ? await _roomRights.CanManageStickiesAsync(account, room.Data, cancellationToken)
            : await _roomRights.CanUseFurnitureAsync(account, room.Data, cancellationToken);
        if (!mayPlace)
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.Generic).Compose());
            return;
        }

        if (item.LogicClass == FurnitureLogicClass.RoomDimmer && room.CountWallItemsWithLogicClass(FurnitureLogicClass.RoomDimmer) > 0)
        {
            bool canUseFurniCommand = await _roomRights.HoldsFuseAsync(account, FurniChooserFuse, cancellationToken);
            session.Send(new HabboBroadcastMessageEvent(_texts.Get(MessageKeys.MoodLightOnePerRoom) + (canUseFurniCommand ? _texts.Get(MessageKeys.FurniCommandTip) : string.Empty)).Compose());
            return;
        }

        if (await ExceededLimitAsync(account, room, isSticky: isStickie, isRoller: false, cancellationToken) is { } limit)
        {
            session.Send(new PlaceObjectErrorMessageEvent(limit).Compose());
            return;
        }

        string wallPosition = WallPositionParser.Format(wallX, wallY, offsetX, offsetY, direction);

        if (isStickie)
        {
            await PlaceStickieAsync(session, room, account, roomId, itemId, wallPosition, cancellationToken);
            return;
        }

        FurnitureDefinition? definition = _furnitureDefinitions.GetById(item.FurniDefId);
        FurnitureRollKind rollKind = RollKindOf(definition);

        string? basalExtraData = item.LogicClass == FurnitureLogicClass.RoomDimmer
            ? RoomDimmerState.Basal.ToExtraData()
            : FurnitureRolls.BasalExtraData(rollKind);

        bool placed = await _userItemRepository.PlaceOnWallAsync(
            itemId, account.Id, roomId, wallPosition, basalExtraData, FurnitureRolls.ResetsWhenPlaced(rollKind), cancellationToken);
        if (!placed)
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.ItemNotFound).Compose());
            return;
        }

        session.Send(new FurniListRemoveEvent(itemId).Compose());

        UserItem? placedItem = await _userItemRepository.GetByIdAsync(itemId, cancellationToken);
        if (placedItem is not null)
        {
            room.AddWallItem(placedItem);
            _roomBroadcaster.Broadcast(room, new ItemAddMessageEvent(placedItem).Compose());
            await _rolls.OnPlacedAsync(room, itemId, rollKind, cancellationToken);
        }
    }

    private async ValueTask PlaceStickieAsync(
        IGameSession session, Room room, UserAccount account, int roomId, int padId, string wallPosition, CancellationToken cancellationToken)
    {
        StickiePlacement? placement = await _userItemRepository.PlaceStickieAsync(
            padId, account.Id, roomId, wallPosition, Stickie.DefaultColour, cancellationToken);
        if (placement is null)
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.ItemNotFound).Compose());
            return;
        }

        session.Send(placement.NotesLeft > 0
            ? new PostItPlacedEvent(padId, placement.NotesLeft).Compose()
            : new FurniListRemoveEvent(padId).Compose());

        if (await _userItemRepository.GetByIdAsync(placement.NoteId, cancellationToken) is { } note)
        {
            room.AddWallItem(note);
            _roomBroadcaster.Broadcast(room, new ItemAddMessageEvent(note).Compose());
        }
    }

    private async ValueTask MoveStickieAsync(
        IGameSession session, Room room, UserAccount account, int roomId, int noteId, string wallPosition, CancellationToken cancellationToken)
    {
        if (!await _roomRights.CanManageStickiesAsync(account, room.Data, cancellationToken))
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.Generic).Compose());
            return;
        }

        if (!await _userItemRepository.MoveOnWallAsync(noteId, roomId, wallPosition, cancellationToken)
            || room.SetWallItemPosition(noteId, wallPosition) is not { } moved)
        {
            session.Send(new PlaceObjectErrorMessageEvent(PlaceObjectErrorReason.ItemNotFound).Compose());
            return;
        }

        _roomBroadcaster.Broadcast(room, new ItemUpdateMessageEvent(moved).Compose());
    }

    private static FurnitureRollKind RollKindOf(FurnitureDefinition? definition) =>
        definition is null ? FurnitureRollKind.None : FurnitureRolls.KindOf(definition.LogicClass, definition.VisualizationClass);

    private async Task<PlaceObjectErrorReason?> ExceededLimitAsync(
        UserAccount account, Room room, bool isSticky, bool isRoller, CancellationToken cancellationToken)
    {
        (int maxFurnitureInRoom, int maxStickiesInRoom, int maxQueuetilesInRoom, _) =
            _gameSettings.Current.RoomFurnitureLimits;

        PlaceObjectErrorReason? exceeded = null;
        if (room.FloorItemCount + room.WallItemCount >= maxFurnitureInRoom)
        {
            exceeded = PlaceObjectErrorReason.MaxFurniture;
        }
        else if (isSticky && room.CountWallItemsWithLogicClass(FurnitureLogicClass.Stickie) >= maxStickiesInRoom)
        {
            exceeded = PlaceObjectErrorReason.MaxStickies;
        }
        else if (isRoller && room.CountFloorItemsWithInteractionLogic(FurnitureInteractionLogic.Roller) >= maxQueuetilesInRoom)
        {
            exceeded = PlaceObjectErrorReason.MaxQueueTiles;
        }

        if (exceeded is null || await _roomRights.CanExceedRoomLimitsAsync(account, cancellationToken))
        {
            return null;
        }

        return exceeded;
    }
}
