using System.Globalization;
using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Rooms;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.RoomAction;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Protocol.Messages.Outgoing.RoomSession;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Furniture;

public sealed class FreezeService
{
    private const int ClockStartStop = 1;
    private const int ClockNextPreset = 2;

    private readonly RoomBroadcaster _broadcaster;
    private readonly IUserItemRepository _userItems;
    private readonly AchievementService _achievements;
    private readonly ClientSessionManager _sessions;
    private readonly ILogger<FreezeService> _logger;

    public FreezeService(RoomBroadcaster broadcaster, IUserItemRepository userItems, AchievementService achievements, ClientSessionManager sessions,
        ILogger<FreezeService> logger)
    {
        _broadcaster = broadcaster;
        _userItems = userItems;
        _achievements = achievements;
        _sessions = sessions;
        _logger = logger;
    }

    public void HandleTick(Room room, FreezeTickResult result)
    {
        if (ObjectsDataUpdateMessageEvent.ForChanges(result.Changes.Select(change => (change.ItemId, change.Data)).ToList()) is { } update)
        {
            _broadcaster.Broadcast(room, update.Compose());
        }

        foreach (FreezeChange change in result.Changes)
        {
            if (change.Save)
            {
                Background(_userItems.UpdateExtraDataAsync(change.ItemId, change.Data), "Saving Freeze's state failed.");
            }
        }

        foreach ((long sessionId, bool playing) in result.Playing)
        {
            if (_sessions.TryGet(sessionId, out ClientSession? session))
            {
                session!.EnqueueSend(new YouArePlayingGameMessageEvent(playing).Compose());
            }
        }

        foreach ((int virtualId, int lives) in result.Lives)
        {
            _broadcaster.Broadcast(room, new GamePlayerValueMessageEvent(virtualId, lives).Compose());
        }

        foreach (int virtualId in result.Waves)
        {
            _broadcaster.Broadcast(room, new WaveMessageEvent(virtualId).Compose());
        }

        if (result.StoppedClockId is { } clockId && room.GetClock(clockId) is { } clock)
        {
            room.SetClock(clockId, clock.Seconds, running: false);
            Background(_userItems.UpdateExtraDataAsync(clockId, clock.Seconds.ToString(CultureInfo.InvariantCulture)), "Saving Freeze's clock failed.");
        }

        if (result.PowerUps.Count > 0 || result.Games.Count > 0)
        {
            Background(AwardAsync(result.PowerUps, result.Games), "Freeze's achievements failed.");
        }
    }

    public async Task AwardAsync(IReadOnlyList<int> powerUps, IReadOnlyList<FreezeGameResult> games)
    {
        foreach (IGrouping<int, int> taken in powerUps.GroupBy(accountId => accountId))
        {
            await _achievements.IncrementAsync(taken.Key, FreezeRules.PowerUpAchievement, taken.Count());
        }

        foreach (FreezeGameResult game in games)
        {
            foreach ((int accountId, int freezes) in game.Freezes)
            {
                await _achievements.IncrementAsync(accountId, FreezeRules.FighterAchievement, freezes);
            }

            foreach (FreezePlayerResult player in game.Players.Where(player => player.TeamScore > 0))
            {
                await _achievements.IncrementAsync(player.AccountId, FreezeRules.PlayerAchievement, player.TeamScore);
                if (player.Won)
                {
                    await _achievements.IncrementAsync(player.AccountId, FreezeRules.WinnerAchievement, player.TeamScore);
                    await _achievements.IncrementAsync(player.AccountId, BanzaiService.GamePlayerExperienceAchievement, player.TeamScore);
                }
            }
        }
    }

    public bool Throw(Room room, RoomActor actor, RoomFloorItem item) => room.TryThrowSnowball(actor, item.Id);

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
                    room.PauseFreeze();
                    await _userItems.UpdateExtraDataAsync(clock.Id, seconds.ToString(CultureInfo.InvariantCulture), cancellationToken);
                    return;
                }

                if (seconds <= 0 || room.FreezePhase is FreezePhase.Over or FreezePhase.Idle)
                {
                    seconds = room.ClockPreset(clock.Id);
                    await ShowAndSaveAsync(room, clock.Id, seconds, cancellationToken);
                }

                room.SetClock(clock.Id, seconds, running: true);
                if (!room.ResumeFreeze(clock.Id))
                {
                    room.StartFreeze(clock.Id);
                }

                break;

            case ClockNextPreset:
                if (running)
                {
                    return;
                }

                if (room.StopFreeze())
                {
                    int again = room.ClockPreset(clock.Id);
                    room.SetClock(clock.Id, again, running: false);
                    await ShowAndSaveAsync(room, clock.Id, again, cancellationToken);
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

    private void Background(Task work, string failure) =>
        _ = work.ContinueWith(t => _logger.LogError(t.Exception, "{Failure}", failure), TaskContinuationOptions.OnlyOnFaulted);
}
