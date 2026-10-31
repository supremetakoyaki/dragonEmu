using System.Globalization;
using System.Threading.Channels;
using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Protocol.Messages.Outgoing.Sound;
using DragonEmu.Protocol.Messages.Outgoing.Idol;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Rooms;

public sealed class IdolService : IHostedService
{
    public const string VoteAchievement = "ACH_AIPerformanceVote";

    private readonly IRoomManager _roomManager;
    private readonly ITraxRepository _trax;
    private readonly IUserItemRepository _userItems;
    private readonly RoomBroadcaster _broadcaster;
    private readonly ClientSessionManager _sessions;
    private readonly AchievementService _achievements;
    private readonly ILogger<IdolService> _logger;

    private readonly Channel<Func<CancellationToken, Task>> _work =
        Channel.CreateUnbounded<Func<CancellationToken, Task>>(new UnboundedChannelOptions { SingleReader = true });

    private Task? _workLoop;

    public IdolService(
        IRoomManager roomManager, ITraxRepository trax, IUserItemRepository userItems, RoomBroadcaster broadcaster,
        ClientSessionManager sessions, AchievementService achievements, ILogger<IdolService> logger)
    {
        _roomManager = roomManager;
        _trax = trax;
        _userItems = userItems;
        _broadcaster = broadcaster;
        _sessions = sessions;
        _achievements = achievements;
        _logger = logger;
    }

    public void HandleTick(Room room, IdolTickResult tick)
    {
        if (tick == IdolTickResult.None)
        {
            return;
        }

        foreach (long judge in tick.JudgesLeft)
        {
            SendTo(judge, JudgeGuiStatusMessageEvent.Closed());
        }

        if (tick.PerformerLeft is { } left)
        {
            SendTo(left, new ClosePerformerGuiMessageEvent());
        }

        if (tick.PerformanceEnded)
        {
            EndPerformance(room);
        }

        foreach (long judge in tick.JudgesSat)
        {
            SendTo(judge, JudgeStatusFor(room, judge));
        }

        if (tick.PerformerArrived is { } performer && _sessions.TryGet(performer, out ClientSession? session) && session!.AuthenticatedUser is { } account)
        {
            Enqueue(async cancellationToken =>
            {
                IReadOnlyList<TraxSong> songs = await _trax.GetUserSongsAsync(account.Id, cancellationToken);
                if (room.Performer == performer && !room.IsPerforming)
                {
                    session.EnqueueSend(new OpenPerformerGuiMessageEvent(songs).Compose());
                }
            });
        }

        foreach (FurnitureStateChange change in tick.StateChanges)
        {
            SetState(room, change.ItemId, change.ExtraData);
        }
    }

    public async Task StartPerformanceAsync(IGameSession session, UserAccount account, int songId, CancellationToken cancellationToken)
    {
        if (session.CurrentRoomId is not { } roomId || !_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || room!.Performer is not { } performer || performer != session.Id || room.Hotspot is not { } hotspot)
        {
            return;
        }

        TraxSong? song = songId == 0 ? null
            : (await _trax.GetUserSongsAsync(account.Id, cancellationToken)).FirstOrDefault(own => own.Id == songId);
        DateTime now = DateTime.UtcNow;
        if (!room.TryStartPerformance(performer, song is null ? null : TimeSpan.FromSeconds(song.LengthSeconds), now))
        {
            return;
        }

        session.Send(new ClosePerformerGuiMessageEvent().Compose());
        if (song is not null)
        {
            room.StartPlayback(hotspot.Id, [song], now);
            _broadcaster.Broadcast(room, new PlayListMessageEvent(0, [song]).Compose());
        }

        SetState(room, hotspot.Id, "0");
        JudgeGuiStatusMessageEvent voting = JudgeGuiStatusMessageEvent.Voting(account.Id, account.Username, account.Figure);
        foreach ((long judge, _) in room.Judges)
        {
            SendTo(judge, voting);
        }
    }

    public void Vote(IGameSession session, bool yes)
    {
        if (session.CurrentRoomId is not { } roomId || !_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || room!.Hotspot is not { } hotspot || room.Performer is not { } performer
            || !_sessions.TryGet(performer, out ClientSession? performing) || performing!.AuthenticatedUser is not { } performerAccount)
        {
            return;
        }

        if (room.TryVote(session.Id, yes, DateTime.UtcNow) is not { } vote)
        {
            return;
        }

        SetState(room, hotspot.Id, vote.Votes.ToString(CultureInfo.InvariantCulture));
        SetState(room, vote.ChairId, vote.ChairState);
        Enqueue(cancellationToken => _achievements.IncrementAsync(performerAccount.Id, VoteAchievement, cancellationToken: cancellationToken));
    }

    private void EndPerformance(Room room)
    {
        if (room.Hotspot is not { } hotspot)
        {
            return;
        }

        if (room.Playback?.MachineItemId == hotspot.Id)
        {
            room.StopPlayback();
        }

        SetState(room, hotspot.Id, Room.HotspotOff);
        Enqueue(async cancellationToken =>
        {
            if (await _userItems.GetByIdAsync(hotspot.Id, cancellationToken) is { } current && current.RoomRef == room.Id)
            {
                _broadcaster.Broadcast(room, new ObjectRemoveMessageEvent(hotspot.Id).Compose());
                _broadcaster.Broadcast(room, new ObjectAddMessageEvent(current).Compose());
            }
        });

        foreach ((long judge, _) in room.Judges)
        {
            SendTo(judge, JudgeGuiStatusMessageEvent.Waiting());
        }
    }

    private JudgeGuiStatusMessageEvent JudgeStatusFor(Room room, long judge)
    {
        if (room.IsPerforming && room.Performer is { } performer && _sessions.TryGet(performer, out ClientSession? performing)
            && performing!.AuthenticatedUser is { } account && !room.Judges.Any(entry => entry.SessionId == judge && entry.Voted))
        {
            return JudgeGuiStatusMessageEvent.Voting(account.Id, account.Username, account.Figure);
        }

        return JudgeGuiStatusMessageEvent.Waiting();
    }

    private void SetState(Room room, int itemId, string state)
    {
        _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(itemId, state).Compose());
        Enqueue(cancellationToken => _userItems.UpdateExtraDataAsync(itemId, state, cancellationToken));
    }

    private void SendTo(long sessionId, IMessageEvent message)
    {
        if (_sessions.TryGet(sessionId, out ClientSession? session))
        {
            session!.EnqueueSend(message.Compose());
        }
    }

    private void Enqueue(Func<CancellationToken, Task> work) => _work.Writer.TryWrite(work);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _workLoop = Task.Run(WorkAsync, CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _work.Writer.TryComplete();
        if (_workLoop is not null)
        {
            try
            {
                await _workLoop.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    private async Task WorkAsync()
    {
        await foreach (Func<CancellationToken, Task> work in _work.Reader.ReadAllAsync())
        {
            try
            {
                await work(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something the Idol did failed.");
            }
        }
    }
}
