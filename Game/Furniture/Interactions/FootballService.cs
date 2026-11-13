using System.Globalization;
using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.RoomAction;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Furniture;

public sealed class FootballService
{
    private const int CounterAdd = 1;
    private const int CounterTake = 2;
    private const int CounterReset = 3;
    private const int ClockStartStop = 1;
    private const int ClockNextPreset = 2;

    private readonly RoomBroadcaster _broadcaster;
    private readonly IUserItemRepository _userItems;
    private readonly ILogger<FootballService> _logger;

    public FootballService(RoomBroadcaster broadcaster, IUserItemRepository userItems, ILogger<FootballService> logger)
    {
        _broadcaster = broadcaster;
        _userItems = userItems;
        _logger = logger;
    }

    public void HandleTick(Room room, FootballTickResult result, IReadOnlyList<BallMove> nudges)
    {
        foreach (BallMove move in nudges)
        {
            _broadcaster.Broadcast(room, new ObjectUpdateMessageEvent(move.Ball, move.State).Compose());
            Save(SaveBallAsync(room, move.Ball, move.State));
        }

        foreach (BallMove move in result.Moves)
        {
            _broadcaster.Broadcast(room, new ObjectUpdateMessageEvent(move.Ball, move.State).Compose());
        }

        var changes = new List<(int Id, string Data)>();
        changes.AddRange(result.StateChanges.Select(change => (change.ItemId, change.ExtraData)));
        changes.AddRange(result.Goals.Select(goal => (goal.Goal.Id, "1")));
        if (ObjectsDataUpdateMessageEvent.ForChanges(changes) is { } update)
        {
            _broadcaster.Broadcast(room, update.Compose());
        }

        foreach (FootballGoalScored goal in result.Goals)
        {
            if (goal.Kicker is { } kicker)
            {
                lock (kicker.Lock)
                {
                    kicker.DanceId = 0;
                }

                _broadcaster.Broadcast(room, new WaveMessageEvent(kicker.VirtualId).Compose());
            }

            Save(ScoreAsync(room, goal.Team));
        }

        foreach (BallRest rest in result.Rests)
        {
            Save(SaveBallAsync(room, rest.Ball, rest.State));
        }

        foreach (CountdownFinished clock in result.Finished)
        {
            Save(_userItems.UpdateExtraDataAsync(clock.ItemId, "0"));
        }
    }

    public async Task UseCounterAsync(Room room, RoomFloorItem counter, int param, CancellationToken cancellationToken)
    {
        int? next = param switch
        {
            CounterAdd => FootballRules.NextScore(await CurrentValueAsync(room, counter.Id, cancellationToken)),
            CounterTake => FootballRules.PreviousScore(await CurrentValueAsync(room, counter.Id, cancellationToken)),
            CounterReset => 0,
            _ => null,
        };

        if (next is { } score)
        {
            await ShowAndSaveAsync(room, counter.Id, score, cancellationToken);
        }
    }

    public async Task UseClockAsync(Room room, RoomFloorItem clock, int param, CancellationToken cancellationToken)
    {
        (int seconds, bool running) = room.GetClock(clock.Id) ?? (await CurrentValueAsync(room, clock.Id, cancellationToken), false);

        switch (param)
        {
            case ClockStartStop:
                if (!running && seconds <= 0)
                {
                    return;
                }

                room.SetClock(clock.Id, seconds, running: !running);
                await _userItems.UpdateExtraDataAsync(clock.Id, seconds.ToString(CultureInfo.InvariantCulture), cancellationToken);
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

    private async Task ScoreAsync(Room room, FootballTeam team)
    {
        if (team == FootballTeam.None)
        {
            return;
        }

        IReadOnlyList<RoomFloorItem> counters = room.FloorItemsWhere(item =>
            item.InteractionLogic == FurnitureInteractionLogic.FootballScoreCounter && FootballTeams.OfClassname(item.Classname) == team);
        if (counters.Count == 0)
        {
            return;
        }

        IReadOnlyDictionary<int, string?> values = await _userItems.GetExtraDataInRoomAsync(room.Id, counters.Select(c => c.Id).ToList());
        var shown = new List<(int Id, string Data)>(counters.Count);
        foreach (RoomFloorItem counter in counters)
        {
            string data = FootballRules.NextScore(Parse(values.GetValueOrDefault(counter.Id))).ToString(CultureInfo.InvariantCulture);
            await _userItems.UpdateExtraDataAsync(counter.Id, data);
            shown.Add((counter.Id, data));
        }

        if (ObjectsDataUpdateMessageEvent.ForChanges(shown) is { } update)
        {
            _broadcaster.Broadcast(room, update.Compose());
        }
    }

    private async Task ShowAndSaveAsync(Room room, int itemId, int value, CancellationToken cancellationToken)
    {
        string data = value.ToString(CultureInfo.InvariantCulture);
        await _userItems.UpdateExtraDataAsync(itemId, data, cancellationToken);
        _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(itemId, data).Compose());
    }

    private async Task<int> CurrentValueAsync(Room room, int itemId, CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<int, string?> values = await _userItems.GetExtraDataInRoomAsync(room.Id, [itemId], cancellationToken);
        return Parse(values.GetValueOrDefault(itemId));
    }

    private static int Parse(string? extraData) =>
        int.TryParse(extraData, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : 0;

    private Task SaveBallAsync(Room room, RoomFloorItem ball, string state) =>
        Task.WhenAll(
            _userItems.MoveOnFloorAsync(ball.Id, room.Id, ball.X, ball.Y, (decimal)ball.Z, ball.Rotation),
            _userItems.UpdateExtraDataAsync(ball.Id, state));

    private void Save(Task saving) =>
        _ = saving.ContinueWith(t => _logger.LogError(t.Exception, "Saving the football's state failed."), TaskContinuationOptions.OnlyOnFaulted);
}
