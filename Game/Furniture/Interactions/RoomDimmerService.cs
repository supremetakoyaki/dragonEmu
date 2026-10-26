using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Protocol.Messages.Outgoing.RoomFurniture;

namespace DragonEmu.Game.Furniture;

public sealed class RoomDimmerService
{
    private readonly IRoomManager _roomManager;
    private readonly IUserItemRepository _userItemRepository;
    private readonly IDimmerPresetRepository _presetRepository;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly RoomRightsService _roomRights;

    public RoomDimmerService(
        IRoomManager roomManager, IUserItemRepository userItemRepository, IDimmerPresetRepository presetRepository,
        RoomBroadcaster roomBroadcaster, RoomRightsService roomRights)
    {
        _roomManager = roomManager;
        _userItemRepository = userItemRepository;
        _presetRepository = presetRepository;
        _roomBroadcaster = roomBroadcaster;
        _roomRights = roomRights;
    }

    public async Task SendPresetsAsync(IGameSession session, CancellationToken cancellationToken = default)
    {
        if (await ResolveAsync(session, cancellationToken) is not (_, UserItem dimmer))
        {
            return;
        }

        IReadOnlyList<DimmerPreset> presets = await _presetRepository.GetPresetsAsync(dimmer.Id, cancellationToken);
        session.Send(new RoomDimmerPresetsMessageEvent(RoomDimmerState.Parse(dimmer.ExtraData).PresetId, presets).Compose());
    }

    public async Task SavePresetAsync(
        IGameSession session, int presetId, int type, string color, int intensity, bool apply, CancellationToken cancellationToken = default)
    {
        if (!RoomDimmers.IsValidPreset(presetId, type, color, intensity)
            || await ResolveAsync(session, cancellationToken) is not (Room room, UserItem dimmer))
        {
            return;
        }

        var preset = new DimmerPreset(presetId, type, color.ToUpperInvariant(), intensity);
        await _presetRepository.SavePresetAsync(dimmer.Id, preset, cancellationToken);

        if (apply)
        {
            await ApplyAsync(room, dimmer, RoomDimmerState.Parse(dimmer.ExtraData).WithPreset(preset) with { IsOn = true }, cancellationToken);
        }
    }

    public async Task ToggleAsync(IGameSession session, CancellationToken cancellationToken = default)
    {
        if (await ResolveAsync(session, cancellationToken) is not (Room room, UserItem dimmer))
        {
            return;
        }

        RoomDimmerState state = RoomDimmerState.Parse(dimmer.ExtraData);
        IReadOnlyList<DimmerPreset> presets = await _presetRepository.GetPresetsAsync(dimmer.Id, cancellationToken);
        DimmerPreset selected = presets.FirstOrDefault(preset => preset.Id == state.PresetId) ?? RoomDimmers.DefaultPreset(state.PresetId);

        await ApplyAsync(room, dimmer, state.WithPreset(selected) with { IsOn = !state.IsOn }, cancellationToken);
    }

    private async Task ApplyAsync(Room room, UserItem dimmer, RoomDimmerState state, CancellationToken cancellationToken)
    {
        string extraData = state.ToExtraData();
        await _userItemRepository.UpdateExtraDataAsync(dimmer.Id, extraData, cancellationToken);

        if (room.SetWallItemExtraData(dimmer.Id, extraData) is { } updated)
        {
            _roomBroadcaster.Broadcast(room, new ItemUpdateMessageEvent(updated).Compose());
        }
    }

    private async Task<(Room Room, UserItem Dimmer)?> ResolveAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account
            || session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room) || room is null
            || room.GetWallItemWithLogicClass(FurnitureLogicClass.RoomDimmer) is not { } dimmer
            || !await _roomRights.CanToggleFurnitureAsync(account, room.Data, cancellationToken))
        {
            return null;
        }

        return (room, dimmer);
    }
}
