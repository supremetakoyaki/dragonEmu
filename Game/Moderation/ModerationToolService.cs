using DragonEmu.Domain;
using DragonEmu.Game.Fuses;
using DragonEmu.Game.History;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Settings;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Help;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;

namespace DragonEmu.Game.Moderation;

public sealed class ModerationToolService
{
    public const string ModeratorFuse = "fuse_moderator_access";
    public const string ChatlogFuse = "fuse_chat_log";
    public const string AlertFuse = "fuse_alert";
    public const string BanFuse = "fuse_ban";
    public const string RoomAlertFuse = "fuse_room_alert";
    public const string RoomKickFuse = "fuse_room_kick";

    private const int RoomChatlogLines = 150;
    private const int UserChatlogVisits = 5;
    private const int LinesPerVisit = 50;
    private const int RoomVisits = 50;
    private const int MaxMessageLength = 1000;
    private static readonly TimeSpan CallChatlogWindow = TimeSpan.FromMinutes(5);

    private readonly FuseService _fuses;
    private readonly ModerationPresetsCache _presets;
    private readonly ModerationIssueService _issues;
    private readonly IModerationToolRepository _repository;
    private readonly IUserRepository _users;
    private readonly IPrivateRoomRepository _rooms;
    private readonly IRoomManager _roomManager;
    private readonly ClientSessionManager _sessions;
    private readonly RoomBroadcaster _broadcaster;
    private readonly RoomKickService _kicks;
    private readonly ModerationService _moderation;
    private readonly HotelClock _clock;
    private readonly HistoryLog _history;
    private readonly MessageTexts _texts;

    private string NoSuchUser => _texts.Get(MessageKeys.NoSuchUser);

    private string NoMessage => _texts.Get(MessageKeys.NoMessage);

    public ModerationToolService(
        FuseService fuses, ModerationPresetsCache presets, ModerationIssueService issues, IModerationToolRepository repository, IUserRepository users,
        IPrivateRoomRepository rooms, IRoomManager roomManager, ClientSessionManager sessions, RoomBroadcaster broadcaster, RoomKickService kicks,
        ModerationService moderation, HotelClock clock, HistoryLog history, MessageTexts texts)
    {
        _texts = texts;
        _fuses = fuses;
        _presets = presets;
        _issues = issues;
        _repository = repository;
        _users = users;
        _rooms = rooms;
        _roomManager = roomManager;
        _sessions = sessions;
        _broadcaster = broadcaster;
        _kicks = kicks;
        _moderation = moderation;
        _clock = clock;
        _history = history;
    }

    public async Task SendInitAsync(IGameSession session, UserAccount account, CancellationToken cancellationToken)
    {
        if (!await HoldsAsync(account, ModeratorFuse, cancellationToken))
        {
            return;
        }

        session.IsModerator = true;
        var permissions = new ModeratorPermissions(
            Tickets: true,
            Chatlogs: await HoldsAsync(account, ChatlogFuse, cancellationToken),
            AlertUser: await HoldsAsync(account, AlertFuse, cancellationToken),
            KickUser: true,
            BanUser: await HoldsAsync(account, BanFuse, cancellationToken),
            AlertRoom: await HoldsAsync(account, RoomAlertFuse, cancellationToken),
            KickRoom: await HoldsAsync(account, RoomKickFuse, cancellationToken),
            WordFilter: await HoldsAsync(account, RoomRightsService.AdministratorAccessFuse, cancellationToken));
        session.Send(new ModeratorInitMessageEvent(_issues.Active, _presets.Current, permissions, DateTime.UtcNow).Compose());
    }

    public async Task SendUserInfoAsync(IGameSession session, UserAccount moderator, int userId, CancellationToken cancellationToken)
    {
        if (await HoldsAsync(moderator, ModeratorFuse, cancellationToken) && await _repository.GetUserInfoAsync(userId, cancellationToken) is { } info)
        {
            session.Send(new ModeratorUserInfoEvent(info, _sessions.IsOnline(userId), DateTime.UtcNow).Compose());
        }
    }

    public async Task SendRoomVisitsAsync(IGameSession session, UserAccount moderator, int userId, CancellationToken cancellationToken)
    {
        if (!await HoldsAsync(moderator, ModeratorFuse, cancellationToken) || await _users.GetByIdAsync(userId, cancellationToken) is not { } user)
        {
            return;
        }

        await _history.FlushRoomVisitsAsync();
        List<RoomVisit> visits = await VisitsAsync(userId, RoomVisits, cancellationToken);
        session.Send(new RoomVisitsEvent(user.Id, user.Username, visits, _clock.ToHotelTime).Compose());
    }

    public async Task SendUserChatlogAsync(IGameSession session, UserAccount moderator, int userId, CancellationToken cancellationToken)
    {
        if (!await HoldsAsync(moderator, ChatlogFuse, cancellationToken) || await _users.GetByIdAsync(userId, cancellationToken) is not { } user)
        {
            return;
        }

        await _history.FlushRoomVisitsAsync();
        await _history.FlushRoomChatAsync();
        var rooms = new List<ChatlogRoom>();
        if (CurrentVisit(userId) is { } current
            && await _repository.GetRoomChatAsync(current.RoomId, current.EnteredAt, current.LeftAt, LinesPerVisit, cancellationToken) is { Count: > 0 } now)
        {
            rooms.Add(new ChatlogRoom(current.RoomId, current.RoomName, current.IsPublic, now));
        }

        foreach (RoomVisit visit in await _repository.GetVisitsWithChatAsync(userId, UserChatlogVisits - rooms.Count, cancellationToken))
        {
            IReadOnlyList<ChatlogLine> lines = await _repository.GetRoomChatAsync(visit.RoomId, visit.EnteredAt, visit.LeftAt, LinesPerVisit, cancellationToken);
            rooms.Add(new ChatlogRoom(visit.RoomId, visit.RoomName, visit.IsPublic, lines));
        }

        session.Send(new UserChatlogEvent(user.Id, user.Username, rooms, _clock.ToHotelTime).Compose());
    }

    public async Task SendRoomChatlogAsync(IGameSession session, UserAccount moderator, int roomId, CancellationToken cancellationToken)
    {
        if (await HoldsAsync(moderator, ChatlogFuse, cancellationToken) && await _rooms.GetByIdAsync(roomId, cancellationToken) is { } room)
        {
            await _history.FlushRoomChatAsync();
            IReadOnlyList<ChatlogLine> lines = await _repository.GetRoomChatAsync(roomId, null, null, RoomChatlogLines, cancellationToken);
            session.Send(new RoomChatlogEvent(new ChatlogRoom(room.Id, room.Label, room.OwnerId is null, lines), _clock.ToHotelTime).Compose());
        }
    }

    public async Task SendCallChatlogAsync(IGameSession session, UserAccount moderator, int issueId, CancellationToken cancellationToken)
    {
        if (!await HoldsAsync(moderator, ChatlogFuse, cancellationToken) || _issues.Get(issueId) is not { } issue)
        {
            return;
        }

        await _history.FlushRoomChatAsync();
        IReadOnlyList<ChatlogLine> lines = issue.RoomId is { } roomId
            ? await _repository.GetRoomChatAsync(roomId, issue.CreatedAt - CallChatlogWindow, issue.CreatedAt, RoomChatlogLines, cancellationToken)
            : [];
        session.Send(new CfhChatlogEvent(issue, new ChatlogRoom(issue.RoomId ?? 0, issue.RoomName, issue.RoomIsPublic, lines), _clock.ToHotelTime).Compose());
    }

    public async Task SendRoomInfoAsync(IGameSession session, UserAccount moderator, int roomId, CancellationToken cancellationToken)
    {
        if (!await HoldsAsync(moderator, ModeratorFuse, cancellationToken) || await _rooms.GetByIdAsync(roomId, cancellationToken) is not { } data)
        {
            return;
        }

        bool loaded = _roomManager.TryGetLoadedRoom(roomId, out Room? room);
        bool ownerInRoom = loaded && data.OwnerId is { } ownerId && room!.Actors.Any(actor => actor.Type == ActorType.Player && actor.AccountId == ownerId);
        session.Send(new ModeratorRoomInfoEvent(loaded ? room!.Data : data, loaded ? room!.UserCount : 0, ownerInRoom, loaded ? room!.Event : null).Compose());
    }

    public async Task CautionAsync(IGameSession session, UserAccount moderator, int userId, string message, string issueMessage, CancellationToken cancellationToken)
    {
        if (!await HoldsAsync(moderator, AlertFuse, cancellationToken))
        {
            Failed(session, userId);
            return;
        }

        (ClientSession? target, string refusal) = await OnlineLowerRankedAsync(moderator, userId, "caution", cancellationToken);
        if (target is null || Text(message) is not { } text)
        {
            Refuse(session, target is null ? refusal : NoMessage);
            return;
        }

        target.EnqueueSend(new ModMessageEvent(text).Compose());
        await DoneAsync(session, moderator, userId, ModerationActionType.Caution, message, issueMessage, cancellationToken);
    }

    public async Task MessageAsync(IGameSession session, UserAccount moderator, int userId, string message, string issueMessage, CancellationToken cancellationToken)
    {
        if (!await HoldsAsync(moderator, AlertFuse, cancellationToken))
        {
            Failed(session, userId);
            return;
        }

        if (!_sessions.TryGetByUserId(userId, out ClientSession? target) || Text(message) is not { } text)
        {
            Refuse(session, target is null ? await NotOnlineAsync(userId, cancellationToken) : NoMessage);
            return;
        }

        target!.EnqueueSend(issueMessage.Length > 0 && _issues.IsCallerOf(userId, issueMessage)
            ? new CallForHelpReplyMessageEvent(text).Compose()
            : new ModMessageEvent(text).Compose());
        await DoneAsync(session, moderator, userId, ModerationActionType.Message, message, issueMessage, cancellationToken);
    }

    public async Task KickAsync(IGameSession session, UserAccount moderator, int userId, string message, string issueMessage, CancellationToken cancellationToken)
    {
        if (!await HoldsAsync(moderator, ModeratorFuse, cancellationToken))
        {
            Failed(session, userId);
            return;
        }

        (ClientSession? target, string refusal) = await OnlineLowerRankedAsync(moderator, userId, "kick", cancellationToken);
        if (target is null)
        {
            Refuse(session, refusal);
            return;
        }

        if (target.CurrentRoomId is not { } roomId || !_roomManager.TryGetLoadedRoom(roomId, out Room? room) || !room!.TryGetActor(target.Id, out RoomActor? actor))
        {
            Refuse(session, _texts.Format(MessageKeys.UserNotInRoom, ("username", target.AuthenticatedUser!.Username)));
            return;
        }

        Alert(target, message);
        if (!_kicks.PutOutQuietly(room, actor!))
        {
            Failed(session, userId);
            return;
        }

        await DoneAsync(session, moderator, userId, ModerationActionType.Kick, message, issueMessage, cancellationToken);
    }

    public async Task BanAsync(IGameSession session, UserAccount moderator, int userId, string message, int hours, string issueMessage, CancellationToken cancellationToken)
    {
        if (!await HoldsAsync(moderator, BanFuse, cancellationToken))
        {
            Failed(session, userId);
            return;
        }

        UserAccount? target = await _users.GetByIdAsync(userId, cancellationToken);
        string? refusal = target is null ? NoSuchUser : RankRefusal(_texts, moderator, target, "ban") ?? (hours > 0 ? null : _texts.Get(MessageKeys.ChooseBanLength));
        if (refusal is not null)
        {
            Refuse(session, refusal);
            return;
        }

        if (!await _moderation.BanAsync(moderator, target!, hours, Text(message), cancellationToken))
        {
            Failed(session, userId);
            return;
        }

        await DoneAsync(session, moderator, userId, ModerationActionType.Ban, message, issueMessage, cancellationToken);
    }

    public async Task AlertRoomAsync(IGameSession session, UserAccount moderator, int type, string message, CancellationToken cancellationToken)
    {
        if (!await HoldsAsync(moderator, RoomAlertFuse, cancellationToken) || Text(message) is not { } text
            || session.CurrentRoomId is not { } roomId || !_roomManager.TryGetLoadedRoom(roomId, out Room? room))
        {
            return;
        }

        _broadcaster.Broadcast(room!, new ModMessageEvent(text).Compose());
        ModerationActionType action = type is 3 or 4 ? ModerationActionType.RoomMessage : ModerationActionType.RoomCaution;
        await _repository.RecordActionAsync(new ModerationActionRecord(moderator.Id, null, roomId, action, text, null), cancellationToken);
    }

    public async Task ModerateRoomAsync(UserAccount moderator, int roomId, bool lockDoor, bool rename, bool kickEveryone, CancellationToken cancellationToken)
    {
        if (!await HoldsAsync(moderator, ModeratorFuse, cancellationToken) || await _rooms.GetByIdAsync(roomId, cancellationToken) is not { OwnerId: not null } data)
        {
            return;
        }

        _roomManager.TryGetLoadedRoom(roomId, out Room? room);
        if (lockDoor || rename)
        {
            PrivateRoomSummary current = room?.Data ?? data;
            PrivateRoomSummary updated = await _rooms.UpdateSettingsAsync(
                roomId, rename ? _texts.Get(MessageKeys.RoomNameInappropriate) : current.Label, rename ? string.Empty : current.Description, current.CategoryId,
                lockDoor ? RoomAccess.Doorbell : current.AccessType, lockDoor ? null : current.Password, current.MaxUsers,
                rename ? [] : current.Tags, current.AllowPets, current.AllowPetsEat, current.AllowWalkthrough, cancellationToken);
            if (room is not null)
            {
                room.ApplySettingsUpdate(updated);
                if (lockDoor)
                {
                    room.EndEvent();
                }
            }

            if (lockDoor)
            {
                await _repository.RecordActionAsync(new ModerationActionRecord(moderator.Id, null, roomId, ModerationActionType.RoomLock, string.Empty, null), cancellationToken);
            }

            if (rename)
            {
                await _repository.RecordActionAsync(new ModerationActionRecord(moderator.Id, null, roomId, ModerationActionType.RoomRename, current.Label, null), cancellationToken);
            }
        }

        if (kickEveryone && room is not null && await HoldsAsync(moderator, RoomKickFuse, cancellationToken))
        {
            await _kicks.KickEveryoneAsync(room, cancellationToken);
            await _repository.RecordActionAsync(new ModerationActionRecord(moderator.Id, null, roomId, ModerationActionType.RoomKick, string.Empty, null), cancellationToken);
        }
    }

    private async Task DoneAsync(
        IGameSession session, UserAccount moderator, int userId, ModerationActionType action, string message, string issueMessage,
        CancellationToken cancellationToken)
    {
        await _repository.RecordActionAsync(new ModerationActionRecord(moderator.Id, userId, null, action, message, issueMessage), cancellationToken);
        session.Send(new ModeratorActionResultMessageEvent(userId, true).Compose());
    }

    private static void Failed(IGameSession session, int userId) =>
        session.Send(new ModeratorActionResultMessageEvent(userId, false).Compose());

    private static void Refuse(IGameSession session, string why) =>
        session.Send(new HabboBroadcastMessageEvent(why).Compose());

    private async Task<List<RoomVisit>> VisitsAsync(int userId, int limit, CancellationToken cancellationToken)
    {
        var visits = new List<RoomVisit>();
        if (CurrentVisit(userId) is { } current)
        {
            visits.Add(current);
        }

        visits.AddRange(await _repository.GetVisitsAsync(userId, limit - visits.Count, cancellationToken));
        return visits;
    }

    private RoomVisit? CurrentVisit(int userId) =>
        _sessions.TryGetByUserId(userId, out ClientSession? online) && online!.CurrentRoomId is { } roomId
            && _roomManager.TryGetLoadedRoom(roomId, out Room? room) && room!.TryGetActor(online.Id, out RoomActor? actor)
                ? new RoomVisit(room.Id, room.Data.Label, room.Data.OwnerId is null, actor!.EnteredAt, DateTime.UtcNow)
                : null;

    private async Task<(ClientSession? Target, string Refusal)> OnlineLowerRankedAsync(
        UserAccount moderator, int userId, string verb, CancellationToken cancellationToken)
    {
        if (_sessions.TryGetByUserId(userId, out ClientSession? online) && online!.AuthenticatedUser is { } account)
        {
            return RankRefusal(_texts, moderator, account, verb) is { } refusal ? (null, refusal) : (online, string.Empty);
        }

        return await _users.GetByIdAsync(userId, cancellationToken) is { } offline
            ? (null, RankRefusal(_texts, moderator, offline, verb) ?? _texts.Format(MessageKeys.UserNotOnline, ("username", offline.Username)))
            : (null, NoSuchUser);
    }

    private async Task<string> NotOnlineAsync(int userId, CancellationToken cancellationToken) =>
        await _users.GetByIdAsync(userId, cancellationToken) is { } user ? _texts.Format(MessageKeys.UserNotOnline, ("username", user.Username)) : NoSuchUser;

    public static string? RankRefusal(MessageTexts texts, UserAccount moderator, UserAccount target, string verb) =>
        target.Id == moderator.Id ? texts.Format(MessageKeys.RankRefusalYourself, ("verb", verb))
        : target.Rank >= moderator.Rank ? texts.Format(MessageKeys.RankRefusalLowerOnly, ("verb", verb), ("username", target.Username))
        : null;

    private static bool Alert(ClientSession target, string message)
    {
        if (Text(message) is not { } text)
        {
            return false;
        }

        target.EnqueueSend(new ModMessageEvent(text).Compose());
        return true;
    }

    private static string? Text(string message)
    {
        string text = message.Trim();
        return text.Length == 0 ? null : text[..Math.Min(text.Length, MaxMessageLength)];
    }

    private Task<bool> HoldsAsync(UserAccount account, string fuse, CancellationToken cancellationToken) =>
        _fuses.HoldsAsync(account, fuse, cancellationToken: cancellationToken);
}
