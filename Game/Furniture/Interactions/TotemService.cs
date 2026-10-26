using System.Globalization;
using DragonEmu.Domain;
using DragonEmu.Game.Avatars;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Furniture;

public sealed class TotemService
{
    private readonly IUserItemRepository _userItemRepository;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly AvatarEffectService _avatarEffectService;
    private readonly TotemConfigCache _totemConfig;
    private readonly ILogger<TotemService> _logger;

    private readonly PerRoomLock _roomLock = new();

    public TotemService(
        IUserItemRepository userItemRepository, RoomBroadcaster roomBroadcaster, AvatarEffectService avatarEffectService,
        TotemConfigCache totemConfig, ILogger<TotemService> logger)
    {
        _userItemRepository = userItemRepository;
        _roomBroadcaster = roomBroadcaster;
        _avatarEffectService = avatarEffectService;
        _totemConfig = totemConfig;
        _logger = logger;
    }

    public Task UseAsync(IGameSession session, int userId, Room room, RoomFloorItem piece, bool mayChange, CancellationToken cancellationToken) =>
        _roomLock.RunAsync(room, async () =>
        {
            TotemPieces pieces = await ReadAsync(room, cancellationToken);
            if (!pieces.States.TryGetValue(piece.Id, out int state))
            {
                return;
            }

            if (TotemStack.Find(pieces.Items) is { Planet: { } planet } stack && planet.Id == piece.Id)
            {
                if (_totemConfig.Find(pieces.States[stack.Leg.Id], pieces.States[stack.Head.Id], state) is { } combination)
                {
                    await _avatarEffectService.GrantRunningAsync(session, userId, combination.EffectId, combination.EffectDuration, cancellationToken);
                }

                return;
            }

            if (!mayChange)
            {
                return;
            }

            int next = piece.InteractionLogic switch
            {
                FurnitureInteractionLogic.TotemLeg => Totem.NextLegState(state),
                FurnitureInteractionLogic.TotemHead => Totem.NextHeadLook(state),
                _ => Totem.NextPlanetState(state),
            };

            await ApplyAsync(room, pieces, (piece.Id, next), cancellationToken);
        }, cancellationToken);

    public Task SettleAsync(Room room, CancellationToken cancellationToken) =>
        _roomLock.RunAsync(room, async () => await ApplyAsync(room, await ReadAsync(room, cancellationToken), change: null, cancellationToken), cancellationToken);

    public void SettleSoon(Room room) => _ = Task.Run(async () =>
    {
        try
        {
            await SettleAsync(room, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't settle the totem in room {RoomId}.", room.Id);
        }
    });

    public Task PickedUpAsync(Room room, RoomFloorItem piece, CancellationToken cancellationToken) =>
        _roomLock.RunAsync(room, async () =>
        {
            if (piece.InteractionLogic == FurnitureInteractionLogic.TotemHead
                && await _userItemRepository.GetByIdAsync(piece.Id, cancellationToken) is { RoomRef: null } stored
                && ParseState(stored.ExtraData) is var state && Totem.HeadLook(state) != state)
            {
                await _userItemRepository.UpdateExtraDataAsync(piece.Id, Format(Totem.HeadLook(state)), cancellationToken);
            }

            await ApplyAsync(room, await ReadAsync(room, cancellationToken), change: null, cancellationToken);
        }, cancellationToken);

    private async Task ApplyAsync(Room room, TotemPieces pieces, (int ItemId, int State)? change, CancellationToken cancellationToken)
    {
        var states = new Dictionary<int, int>(pieces.States);
        if (change is { } changed)
        {
            states[changed.ItemId] = changed.State;
        }

        TotemStack? stack = TotemStack.Find(pieces.Items);
        foreach (RoomFloorItem head in pieces.Items.Where(piece => piece.InteractionLogic == FurnitureInteractionLogic.TotemHead))
        {
            TotemGlow glow = stack?.Head.Id == head.Id ? Totem.GlowOf(states[stack.Leg.Id]) : TotemGlow.None;
            states[head.Id] = Totem.HeadState(Totem.HeadLook(states[head.Id]), glow);
        }

        foreach ((int itemId, int state) in states)
        {
            if (state == pieces.States[itemId])
            {
                continue;
            }

            string data = Format(state);
            await _userItemRepository.UpdateExtraDataAsync(itemId, data, cancellationToken);
            _roomBroadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(itemId, data).Compose());
        }
    }

    private async Task<TotemPieces> ReadAsync(Room room, CancellationToken cancellationToken)
    {
        IReadOnlyList<RoomFloorItem> placed = room.FloorItemsWhere(item => item.IsTotemPiece);
        IReadOnlyDictionary<int, string?> extraData = await _userItemRepository.GetExtraDataInRoomAsync(
            room.Id, placed.Select(piece => piece.Id).ToArray(), cancellationToken);

        RoomFloorItem[] pieces = placed.Where(piece => extraData.ContainsKey(piece.Id)).ToArray();
        return new TotemPieces(pieces, pieces.ToDictionary(piece => piece.Id, piece => ParseState(extraData[piece.Id])));
    }

    private static int ParseState(string? extraData) =>
        int.TryParse(extraData, NumberStyles.Integer, CultureInfo.InvariantCulture, out int state) ? state : 0;

    private static string Format(int state) => state.ToString(CultureInfo.InvariantCulture);

    private sealed record TotemPieces(IReadOnlyList<RoomFloorItem> Items, IReadOnlyDictionary<int, int> States);
}
