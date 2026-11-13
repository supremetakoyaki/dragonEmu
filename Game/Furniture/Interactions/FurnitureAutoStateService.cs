using System.Globalization;
using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Furniture;

public sealed class FurnitureAutoStateService
{
    private readonly RoomBroadcaster _broadcaster;
    private readonly IUserItemRepository _userItems;
    private readonly ILogger<FurnitureAutoStateService> _logger;

    public FurnitureAutoStateService(RoomBroadcaster broadcaster, IUserItemRepository userItems, ILogger<FurnitureAutoStateService> logger)
    {
        _broadcaster = broadcaster;
        _userItems = userItems;
        _logger = logger;
    }

    public void Apply(Room room, IReadOnlyList<AutoStateChange> changes)
    {
        foreach (AutoStateChange plate in changes.Where(c => c.Item.InteractionLogic == FurnitureInteractionLogic.PressurePlate))
        {
            _broadcaster.Broadcast(room, new ObjectUpdateMessageEvent(plate.Item, State(plate.State)).Compose());
        }

        var shown = changes.Where(c => c.Item.InteractionLogic != FurnitureInteractionLogic.PressurePlate).Select(c => (c.Item.Id, State(c.State))).ToList();
        if (ObjectsDataUpdateMessageEvent.ForChanges(shown) is { } update)
        {
            _broadcaster.Broadcast(room, update.Compose());
        }

        _ = SaveAsync(room, changes).ContinueWith(
            t => _logger.LogError(t.Exception, "Saving the state of pool inflatables / pressure plates failed."), TaskContinuationOptions.OnlyOnFaulted);
    }

    private async Task SaveAsync(Room room, IReadOnlyList<AutoStateChange> changes)
    {
        IReadOnlyDictionary<int, string?> saved = new Dictionary<int, string?>();
        if (changes.Any(c => c.First))
        {
            saved = await _userItems.GetExtraDataInRoomAsync(room.Id, changes.Where(c => c.First).Select(c => c.Item.Id).ToList());
        }

        foreach (AutoStateChange change in changes)
        {
            string data = State(change.State);
            if (change.First && saved.GetValueOrDefault(change.Item.Id) == data)
            {
                continue;
            }

            await _userItems.UpdateExtraDataAsync(change.Item.Id, data);
        }
    }

    private static string State(int state) => state.ToString(CultureInfo.InvariantCulture);
}
