using System.Globalization;
using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Furniture;

public sealed class BanzaiService
{
    private const int ClockStartStop = 1;
    private const int ClockNextPreset = 2;

    public const string TilesLockedAchievement = "ACH_BattleBallTilesLocked";
    public const string PlayerAchievement = "ACH_BattleBallPlayer";
    public const string WinnerAchievement = "ACH_BattleBallWinner";
    public const string GamePlayerExperienceAchievement = "ACH_GamePlayerExperience";

    private readonly RoomBroadcaster _broadcaster;
    private readonly IUserItemRepository _userItems;
    private readonly AchievementService _achievements;
    private readonly ILogger<BanzaiService> _logger;

    public BanzaiService(RoomBroadcaster broadcaster, IUserItemRepository userItems, AchievementService achievements, ILogger<BanzaiService> logger)
    {
        _broadcaster = broadcaster;
        _userItems = userItems;
        _achievements = achievements;
        _logger = logger;
    }

    public void HandleTick(Room room, BanzaiTickResult result)
    {
        foreach (BanzaiChange alone in result.Changes.Where(change => change.Alone))
        {
            _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(alone.ItemId, alone.Data).Compose());
        }

        if (ObjectsDataUpdateMessageEvent.ForChanges(result.Changes.Where(change => !change.Alone).Select(change => (change.ItemId, change.Data)).ToList()) is { } update)
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

        foreach (BanzaiGameResult game in result.Games ?? [])
        {
            Award(AwardAsync(game));
        }
    }

    public async Task AwardAsync(BanzaiGameResult game)
    {
        var points = new Dictionary<int, List<(string CodeBase, int Delta)>>();
        void Add(int accountId, string codeBase, int delta)
        {
            if (delta > 0)
            {
                (points.TryGetValue(accountId, out var list) ? list : points[accountId] = []).Add((codeBase, delta));
            }
        }

        foreach ((int accountId, int tiles) in game.TilesLocked)
        {
            Add(accountId, TilesLockedAchievement, tiles);
        }

        foreach (BanzaiPlayerResult player in game.Players)
        {
            Add(player.AccountId, PlayerAchievement, player.TeamScore);
            if (player.Won)
            {
                Add(player.AccountId, WinnerAchievement, player.TeamScore);
                Add(player.AccountId, GamePlayerExperienceAchievement, player.TeamScore);
            }
        }

        foreach ((int accountId, List<(string CodeBase, int Delta)> earned) in points)
        {
            foreach ((string codeBase, int delta) in earned)
            {
                await _achievements.IncrementAsync(accountId, codeBase, delta);
            }
        }
    }

    public async Task UseApparatusAsync(Room room, RoomFloorItem apparatus, CancellationToken cancellationToken)
    {
        if (room.ToggleApparatus(apparatus.Id) is not { } state)
        {
            return;
        }

        string data = state.ToString(CultureInfo.InvariantCulture);
        _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(apparatus.Id, data).Compose());
        await _userItems.UpdateExtraDataAsync(apparatus.Id, data, cancellationToken);
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

    private void Award(Task awarding) =>
        _ = awarding.ContinueWith(t => _logger.LogError(t.Exception, "Battle Banzai's achievements failed."), TaskContinuationOptions.OnlyOnFaulted);
}
