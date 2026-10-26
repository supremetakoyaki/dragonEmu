using System.Globalization;
using DragonEmu.Domain;
using DragonEmu.Gateway;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.InventoryBadges;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.Polls;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Polls;

public sealed class PollService : IHostedService
{
    public static readonly TimeSpan OfferDelay = TimeSpan.FromMilliseconds(10 * 500);

    private const int MaxAnswersPerQuestion = 50;

    private readonly IPollRepository _polls;
    private readonly ClientSessionManager _sessions;
    private readonly IRoomManager _roomManager;
    private readonly IUserItemRepository _userItems;
    private readonly IUserBadgeRepository _userBadges;
    private readonly ILogger<PollService> _logger;

    public PollService(
        IPollRepository polls, ClientSessionManager sessions, IRoomManager roomManager, IUserItemRepository userItems,
        IUserBadgeRepository userBadges, ILogger<PollService> logger)
    {
        _polls = polls;
        _sessions = sessions;
        _roomManager = roomManager;
        _userItems = userItems;
        _userBadges = userBadges;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _roomManager.RoomLoaded += Watch;
        foreach (Room room in _roomManager.LoadedRooms)
        {
            Watch(room);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _roomManager.RoomLoaded -= Watch;
        return Task.CompletedTask;
    }

    private void Watch(Room room)
    {
        room.UserEntered -= OnUserEntered;
        room.UserEntered += OnUserEntered;
    }

    private void OnUserEntered(Room room, RoomActor actor) => _ = OfferLaterAsync(room.Data.Id, actor.AccountId, actor.SessionId);

    private async Task OfferLaterAsync(int roomId, int userId, long sessionId)
    {
        try
        {
            await Task.Delay(OfferDelay);
            if (!_sessions.TryGet(sessionId, out ClientSession? session)
                || session!.AuthenticatedUser?.Id != userId || session.CurrentRoomId != roomId || session.ActivePoll is not null)
            {
                return;
            }

            if (await _polls.GetUnansweredInRoomAsync(roomId, userId) is { } poll)
            {
                session.EnqueueSend(new PollOfferEvent(poll.Id, poll.Name).Compose());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Offering a poll in room {RoomId} to user {UserId} failed.", roomId, userId);
        }
    }

    public async Task BeginAsync(IGameSession session, UserAccount account, int pollId, CancellationToken cancellationToken)
    {
        if (session.CurrentRoomId is not { } roomId
            || await _polls.GetUnansweredInRoomAsync(roomId, account.Id, cancellationToken) is not { } poll || poll.Id != pollId)
        {
            return;
        }

        IReadOnlyList<PollQuestion> questions = await _polls.GetQuestionsAsync(poll.Id, cancellationToken);
        if (questions.Count == 0)
        {
            return;
        }

        session.ActivePoll = new PollProgress(poll, questions);
        session.Send(new PollContentsEvent(poll, questions).Compose());
    }

    public static void Reject(IGameSession session, int pollId)
    {
        if (session.ActivePoll?.Poll.Id == pollId)
        {
            session.ActivePoll = null;
        }
    }

    public async Task AnswerAsync(
        IGameSession session, UserAccount account, int pollId, int questionId, IReadOnlyList<string> answers, CancellationToken cancellationToken)
    {
        if (session.ActivePoll is not { } progress || progress.Poll.Id != pollId || progress.Current is not { } question || question.Id != questionId
            || session.CurrentRoomId != progress.Poll.RoomId || PollGrader.Grade(question, answers) is not { } graded)
        {
            return;
        }

        progress.Add(graded);
        if (progress.Current is not null)
        {
            return;
        }

        session.ActivePoll = null;
        bool rewarded = Qualifies(progress.Poll, progress.Answers);
        if (!await _polls.CompleteAsync(account.Id, progress.Poll.Id, progress.Answers, progress.Questions.Count, rewarded, cancellationToken))
        {
            return;
        }

        _logger.LogDebug(
            "User {UserId} finished poll {PollId}. {Correct}/{Total} right{Rewarded}.",
            account.Id, progress.Poll.Id, progress.Answers.Count(a => a.IsCorrect), progress.Questions.Count, rewarded ? ", rewarded" : string.Empty);

        if (rewarded)
        {
            await RewardAsync(session, account, progress.Poll, cancellationToken);
        }
    }

    public static bool Qualifies(Poll poll, IReadOnlyList<PollAnswer> answers) =>
        poll.RewardType != PollRewardType.None && (poll.Type == PollType.Survey || answers.All(a => a.IsCorrect));

    private async Task RewardAsync(IGameSession session, UserAccount account, Poll poll, CancellationToken cancellationToken)
    {
        if (poll.RewardType == PollRewardType.Badge)
        {
            if (poll.Reward.Length > 0 && await _userBadges.AwardAsync(account.Id, poll.Reward, cancellationToken))
            {
                session.Send(BadgesEvent.For(await _userBadges.GetAllAsync(account.Id, cancellationToken)).Compose());
            }

            return;
        }

        bool given = false;
        foreach (string part in poll.Reward.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out int definitionId))
            {
                _logger.LogWarning("Poll {PollId}: {Part} in its reward isn't a furniture definition id.", poll.Id, part);
                continue;
            }

            try
            {
                await _userItems.CreateInInventoryAsync(account.Id, definitionId, null, cancellationToken);
                given = true;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Poll {PollId}: giving furniture definition {DefinitionId} failed.", poll.Id, definitionId);
            }
        }

        if (given)
        {
            session.Send(new FurniListUpdateEvent().Compose());
        }
    }
}
