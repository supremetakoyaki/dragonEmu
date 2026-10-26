using System.Globalization;
using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Game.Furniture;

public sealed class FurnitureStateService
{
    public const int BasalState = 0;

    private readonly IUserItemRepository _userItemRepository;
    private readonly RoomBroadcaster _roomBroadcaster;

    public FurnitureStateService(IUserItemRepository userItemRepository, RoomBroadcaster roomBroadcaster)
    {
        _userItemRepository = userItemRepository;
        _roomBroadcaster = roomBroadcaster;
    }

    public static int? NextState(IReadOnlyList<int>? states, string? extraData)
    {
        if (states is not { Count: > 0 })
        {
            return null;
        }

        int current = BasalState;
        if (int.TryParse(extraData, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) && states.Contains(parsed))
        {
            current = parsed;
        }

        int index = -1;
        for (int i = 0; i < states.Count; i++)
        {
            if (states[i] == current)
            {
                index = i;
                break;
            }
        }

        return states[(index + 1) % states.Count];
    }

    public async Task<bool> ToggleNextStateAsync(Room room, RoomFloorItem item, CancellationToken cancellationToken = default)
    {
        if (item.States is not { Count: > 0 })
        {
            return false;
        }

        UserItem? stored = await _userItemRepository.GetByIdAsync(item.Id, cancellationToken);
        if (stored is null || stored.RoomRef != room.Id || NextState(item.States, stored.ExtraData) is not { } next)
        {
            return false;
        }

        string data = next.ToString(CultureInfo.InvariantCulture);
        await _userItemRepository.UpdateExtraDataAsync(item.Id, data, cancellationToken);
        _roomBroadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(item.Id, data).Compose());
        return true;
    }

    public async Task<bool> ToggleNextWallStateAsync(Room room, int itemId, IReadOnlyList<int>? states, CancellationToken cancellationToken = default)
    {
        if (room.GetWallItem(itemId) is not { } item || NextState(states, item.ExtraData) is not { } next)
        {
            return false;
        }

        string data = next.ToString(CultureInfo.InvariantCulture);
        await _userItemRepository.UpdateExtraDataAsync(itemId, data, cancellationToken);
        if (room.SetWallItemExtraData(itemId, data) is { } updated)
        {
            _roomBroadcaster.Broadcast(room, new ItemUpdateMessageEvent(updated).Compose());
        }

        return true;
    }
}
