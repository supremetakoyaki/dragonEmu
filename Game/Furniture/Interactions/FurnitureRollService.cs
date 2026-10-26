using System.Diagnostics.CodeAnalysis;
using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Game.Furniture;

public sealed class FurnitureRollService
{
    private readonly IRoomManager _roomManager;
    private readonly IUserItemRepository _userItemRepository;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly RoomMovementScheduler _scheduler;
    private readonly RoomRightsService _roomRights;

    public FurnitureRollService(
        IRoomManager roomManager, IUserItemRepository userItemRepository, RoomBroadcaster roomBroadcaster,
        RoomMovementScheduler scheduler, RoomRightsService roomRights)
    {
        _roomManager = roomManager;
        _userItemRepository = userItemRepository;
        _roomBroadcaster = roomBroadcaster;
        _scheduler = scheduler;
        _roomRights = roomRights;
    }

    public async Task ThrowDiceAsync(IGameSession session, int itemId, CancellationToken cancellationToken = default)
    {
        if (!TryResolve(session, out Room? room, out RoomActor? actor, out _) || room.GetFloorItem(itemId) is not { } item)
        {
            return;
        }

        switch (KindOf(item))
        {
            case FurnitureRollKind.Dice:
                if (!IsNextTo(actor, item))
                {
                    (int frontX, int frontY) = item.SquareInFront();
                    if (room.TryStartWalk(actor, frontX, frontY))
                    {
                        _scheduler.MarkActive(room);
                    }

                    return;
                }

                await RollAsync(room, itemId, FurnitureRollKind.Dice, cancellationToken);
                break;

            case FurnitureRollKind.Bottle:
                await RollAsync(room, itemId, FurnitureRollKind.Bottle, cancellationToken);
                break;
        }
    }

    public async Task DiceOffAsync(IGameSession session, int itemId, CancellationToken cancellationToken = default)
    {
        if (!TryResolve(session, out Room? room, out RoomActor? actor, out _)
            || room.GetFloorItem(itemId) is not { } item
            || KindOf(item) != FurnitureRollKind.Dice
            || !IsNextTo(actor, item)
            || room.IsRolling(itemId))
        {
            return;
        }

        string basal = FurnitureRolls.BasalExtraData(FurnitureRollKind.Dice)!;
        await _userItemRepository.UpdateExtraDataAsync(itemId, basal, cancellationToken);
        _roomBroadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(itemId, basal).Compose());
    }

    public async Task SpinWheelAsync(IGameSession session, int itemId, CancellationToken cancellationToken = default)
    {
        if (!TryResolve(session, out Room? room, out _, out UserAccount? account)
            || room.GetWallItem(itemId) is not { LogicClass: FurnitureLogicClass.HabboWheel }
            || !await _roomRights.CanToggleFurnitureAsync(account, room.Data, cancellationToken))
        {
            return;
        }

        await RollAsync(room, itemId, FurnitureRollKind.Wheel, cancellationToken);
    }

    public async Task SetRandomStateAsync(IGameSession session, int itemId, CancellationToken cancellationToken = default)
    {
        if (!TryResolve(session, out Room? room, out _, out UserAccount? account) || room.GetFloorItem(itemId) is not { } item)
        {
            return;
        }

        FurnitureRollKind kind = KindOf(item);
        if (kind is not (FurnitureRollKind.ValRandomizer or FurnitureRollKind.Fortune)
            || !await _roomRights.CanToggleFurnitureAsync(account, room.Data, cancellationToken))
        {
            return;
        }

        if (room.IsRolling(itemId))
        {
            if (FurnitureRolls.ResetsWhileShowingResult(kind) && room.TryStopRoll(itemId, FurnitureRolls.RandomizerResultShown))
            {
                _roomBroadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(itemId, FurnitureRolls.BasalExtraData(kind)!).Compose());
            }

            return;
        }

        await RollAsync(room, itemId, kind, cancellationToken);
    }

    public async Task OnPlacedAsync(Room room, int itemId, FurnitureRollKind kind, CancellationToken cancellationToken = default)
    {
        if (kind == FurnitureRollKind.Wheel)
        {
            await RollAsync(room, itemId, kind, cancellationToken, alreadyShowingFirstStep: true);
        }
    }

    private async Task RollAsync(Room room, int itemId, FurnitureRollKind kind, CancellationToken cancellationToken, bool alreadyShowingFirstStep = false)
    {
        (IReadOnlyList<RollStep> steps, string? resting) = FurnitureRolls.Plan(kind, Random.Shared);
        if (alreadyShowingFirstStep)
        {
            steps = steps.Skip(1).ToList();
        }

        if (!room.TryStartRoll(itemId, steps))
        {
            return;
        }

        _scheduler.MarkActive(room);
        await _userItemRepository.UpdateExtraDataAsync(itemId, resting, cancellationToken);
    }

    private bool TryResolve(
        IGameSession session, [NotNullWhen(true)] out Room? room, [NotNullWhen(true)] out RoomActor? actor,
        [NotNullWhen(true)] out UserAccount? account)
    {
        room = null;
        actor = null;
        account = session.AuthenticatedUser;

        if (account is null
            || session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? loadedRoom) || loadedRoom is null
            || !loadedRoom.TryGetActor(session.Id, out RoomActor? loadedActor) || loadedActor is null)
        {
            return false;
        }

        room = loadedRoom;
        actor = loadedActor;
        return true;
    }

    private static FurnitureRollKind KindOf(RoomFloorItem item) => FurnitureRolls.KindOf(item.LogicClass, item.VisualizationClass);

    private static bool IsNextTo(RoomActor actor, RoomFloorItem item)
    {
        lock (actor.Lock)
        {
            return item.IsNextTo(actor.X, actor.Y);
        }
    }
}
