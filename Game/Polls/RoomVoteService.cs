using System.Collections.Concurrent;
using DragonEmu.Domain;
using DragonEmu.Gateway;
using DragonEmu.Game.Chat;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Votes;
using Microsoft.Extensions.Logging;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Polls;

public sealed class RoomVoteService
{
    public const int MaxQuestionLength = 100;

    public static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(60 * 500);

    public static readonly TimeSpan StartCooldown = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan RoomGap = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan AnswerCooldown = TimeSpan.FromMilliseconds(250);

    private const string StartCooldownKey = "vote_start";
    private const string AnswerCooldownKey = "vote_answer";

    private readonly ConcurrentDictionary<int, ActiveVote> _votes = new();
    private readonly ConcurrentDictionary<int, DateTime> _endedAt = new();
    private readonly IRoomManager _roomManager;
    private readonly ClientSessionManager _sessions;
    private readonly ProfanityFilterCache _profanityFilter;
    private readonly ILogger<RoomVoteService> _logger;
    private readonly MessageTexts _texts;

    public RoomVoteService(IRoomManager roomManager, ClientSessionManager sessions, ProfanityFilterCache profanityFilter, ILogger<RoomVoteService> logger, MessageTexts texts)
    {
        _texts = texts;
        _roomManager = roomManager;
        _sessions = sessions;
        _profanityFilter = profanityFilter;
        _logger = logger;
    }

    private sealed record ActiveVote(RoomVote Vote, CancellationTokenSource Timer);

    public string? Start(IGameSession session, UserAccount account, string text)
    {
        if (session.CurrentRoomId is not { } roomId || !_roomManager.TryGetLoadedRoom(roomId, out Room? room) || !room!.TryGetActor(session.Id, out _))
        {
            return _texts.Get(MessageKeys.NotInRoom);
        }

        string question = _profanityFilter.Filter(RoomTextSanitizer.Sanitize(text, MaxQuestionLength)).Trim();
        if (question.Length == 0)
        {
            return _texts.Get(MessageKeys.UsageStartVote);
        }

        if (_votes.ContainsKey(roomId))
        {
            return _texts.Get(MessageKeys.VoteAlreadyGoing);
        }

        if (_endedAt.TryGetValue(roomId, out DateTime ended) && DateTime.UtcNow - ended < RoomGap)
        {
            return _texts.Get(MessageKeys.VoteJustEnded);
        }

        if (!session.TryUseCooldown(StartCooldownKey, StartCooldown))
        {
            return _texts.Format(MessageKeys.VoteStartCooldown, ("seconds", (int)StartCooldown.TotalSeconds));
        }

        IReadOnlyList<int> participants = [.. room.Actors.Where(a => a.Type == ActorType.Player).Select(a => a.AccountId).Distinct()];
        var vote = new RoomVote(roomId, account.Id, question, participants);
        var timer = new CancellationTokenSource();
        if (!_votes.TryAdd(roomId, new ActiveVote(vote, timer)))
        {
            timer.Dispose();
            return _texts.Get(MessageKeys.VoteAlreadyGoing);
        }

        byte[] asked = new VoteQuestionMessageEvent(question, vote.Choices).Compose().ToArray();
        foreach (int participant in participants)
        {
            if (_sessions.TryGetByUserId(participant, out ClientSession? target) && target!.CurrentRoomId == roomId)
            {
                target.EnqueueSend(asked);
            }
        }

        _ = EndAfterAsync(roomId, vote, timer.Token);
        return null;
    }

    public string? EndByCommand(IGameSession session)
    {
        if (session.CurrentRoomId is not { } roomId || !_votes.ContainsKey(roomId))
        {
            return _texts.Get(MessageKeys.VoteNone);
        }

        End(roomId);
        return null;
    }

    public void Answer(IGameSession session, UserAccount account, int choice)
    {
        if (!session.TryUseCooldown(AnswerCooldownKey, AnswerCooldown)
            || session.CurrentRoomId is not { } roomId || !_votes.TryGetValue(roomId, out ActiveVote? active))
        {
            return;
        }

        active.Vote.TryAnswer(account.Id, choice);
    }

    private async Task EndAfterAsync(int roomId, RoomVote vote, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(Duration, cancellationToken);
            End(roomId, vote);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ending the vote in room {RoomId} failed.", roomId);
        }
    }

    private void End(int roomId, RoomVote? only = null)
    {
        if (!_votes.TryGetValue(roomId, out ActiveVote? active) || (only is not null && active.Vote != only)
            || !_votes.TryRemove(new KeyValuePair<int, ActiveVote>(roomId, active)))
        {
            return;
        }

        active.Timer.Cancel();
        active.Timer.Dispose();
        _endedAt[roomId] = DateTime.UtcNow;

        if (active.Vote.End() is not { } counts)
        {
            return;
        }

        byte[] results = new VoteResultMessageEvent(active.Vote.Question, active.Vote.Choices, counts).Compose().ToArray();
        foreach (int participant in active.Vote.Participants)
        {
            if (_sessions.TryGetByUserId(participant, out ClientSession? target) && target!.CurrentRoomId == roomId)
            {
                target.EnqueueSend(results);
            }
        }
    }
}
