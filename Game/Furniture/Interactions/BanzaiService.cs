using System.Globalization;
using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Furniture;

public sealed class BanzaiService
{
    private const int ClockStartStop = 1;
    private const int ClockNextPreset = 2;

    private readonly RoomBroadcaster _broadcaster;
    private readonly IUserItemRepository _userItems;
    private readonly ILogger<BanzaiService> _logger;

    public BanzaiService(RoomBroadcaster broadcaster, IUserItemRepository userItems, ILogger<BanzaiService> logger)
    {
        _broadcaster = broadcaster;
        _userItems = userItems;
        _logger = logger;
    }

    public void HandleTick(Room room, BanzaiTickResult result)
    {
        if (ObjectsDataUpdateMessageEvent.ForChanges(result.Changes.Select(change => (change.ItemId, change.Data)).ToList()) is { } update)
        {
            _broadcaster.Broadcast(room, update.Compose());
        }

        foreach (BanzaiChange change in result.Changes)
        {
            if (change.Save)
            {
                Save(_userItems.UpdateExtraDataAsync(change.ItemId, change.Data));
            }
        }
    }

    public async Task UseClockAsync(Room room, RoomFloorItem clock, int param, CancellationToken cancellationToken)
    {
        if (room.GetClock(clock.Id) is not { } known)
        {
            IReadOnlyDictionary<int, string?> values = await _userItems.GetExtraDataInRoomAsync(room.Id, [clock.Id], cancellationToken);
            int saved = int.TryParse(values.GetValueOrDefault(clock.Id), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : 0;
            room.SetClock(clock.Id, saved, running: false);
            known = (saved, false);
        }

        (int seconds, bool running) = known;
        switch (param)
        {
            case ClockStartStop:
                if (running)
                {
                    room.SetClock(clock.Id, seconds, running: false);
                    room.PauseBanzai();
                    await _userItems.UpdateExtraDataAsync(clock.Id, seconds.ToString(CultureInfo.InvariantCulture), cancellationToken);
                    return;
                }

                if (seconds <= 0)
                {
                    seconds = room.ClockPreset(clock.Id);
                    await ShowAndSaveAsync(room, clock.Id, seconds, cancellationToken);
                }

                room.SetClock(clock.Id, seconds, running: true);
                if (!room.ResumeBanzai(clock.Id))
                {
                    room.StartBanzai(clock.Id);
                }

                break;

            case ClockNextPreset:
                if (!FootballRules.MayChangePreset(running))
                {
                    return;
                }

                int preset = FootballRules.NextCountdownPreset(seconds);
                room.SetClock(clock.Id, preset, running: false);
                await ShowAndSaveAsync(room, clock.Id, preset, cancellationToken);
                break;
        }
    }

    private async Task ShowAndSaveAsync(Room room, int itemId, int value, CancellationToken cancellationToken)
    {
        string data = value.ToString(CultureInfo.InvariantCulture);
        await _userItems.UpdateExtraDataAsync(itemId, data, cancellationToken);
        _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(itemId, data).Compose());
    }

    private void Save(Task saving) =>
        _ = saving.ContinueWith(t => _logger.LogError(t.Exception, "Saving Battle Banzai's state failed."), TaskContinuationOptions.OnlyOnFaulted);
}
