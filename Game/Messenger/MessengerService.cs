using DragonEmu.Game.Moderation;
using System.Globalization;
using DragonEmu.Domain;
using DragonEmu.Game.Chat;
using DragonEmu.Game.History;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Settings;
using DragonEmu.Game.Subscriptions;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.FriendList;
using DragonEmu.Protocol.Messages.Outgoing.RoomSession;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;

namespace DragonEmu.Game.Messenger;

public sealed class MessengerService
{
    public const int MaxInviteLength = 120;

    public const int MaxMessageLength = 255;

    public const int SearchResultLimit = 50;

    private const int MaxIdsPerRequest = 1000;

    private readonly IMessengerRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ClientSessionManager _sessions;
    private readonly GameSettingsCache _gameSettings;
    private readonly SubscriptionManager _subscriptions;
    private readonly ProfanityFilterCache _profanityFilter;
    private readonly IRoomManager _roomManager;
    private readonly HotelClock _clock;
    private readonly HistoryLog _history;
    private readonly MessageTexts _texts;

    public MessengerService(
        IMessengerRepository repository, IUserRepository userRepository, ClientSessionManager sessions, GameSettingsCache gameSettings,
        SubscriptionManager subscriptions, ProfanityFilterCache profanityFilter, IRoomManager roomManager, HotelClock clock,
        HistoryLog history, MessageTexts texts)
    {
        _texts = texts;
        _repository = repository;
        _userRepository = userRepository;
        _sessions = sessions;
        _gameSettings = gameSettings;
        _subscriptions = subscriptions;
        _profanityFilter = profanityFilter;
        _roomManager = roomManager;
        _clock = clock;
        _history = history;
    }

    public async Task InitAsync(IGameSession session, UserAccount me, CancellationToken cancellationToken)
    {
        (int limit, int clubLimit) = (_gameSettings.Current.FriendsLimit, _gameSettings.Current.FriendsLimitClub);
        int myLimit = await _subscriptions.IsClubMemberAsync(me.Id, cancellationToken) ? clubLimit : limit;

        IReadOnlyList<MessengerProfile> friends = await _repository.GetFriendsAsync(me.Id, cancellationToken);
        session.Send(new MessengerInitEvent(myLimit, limit, clubLimit, friends.Select(Entry).ToList()).Compose());
    }

    public async Task AnnounceAsync(int userId, CancellationToken cancellationToken)
    {
        List<ClientSession> onlineFriends = OnlineSessionsOf(await _repository.GetFriendIdsAsync(userId, cancellationToken));
        if (onlineFriends.Count == 0 || await _repository.GetProfileAsync(userId, cancellationToken) is not { } profile)
        {
            return;
        }

        byte[] update = new FriendListUpdateEvent([FriendListChange.Updated(Entry(profile))]).Compose().ToArray();
        foreach (ClientSession friend in onlineFriends)
        {
            friend.EnqueueSend(update);
        }
    }

    public async Task SendRequestsAsync(IGameSession session, UserAccount me, CancellationToken cancellationToken) =>
        session.Send(new BuddyRequestsEvent(await _repository.GetRequestsAsync(me.Id, cancellationToken)).Compose());

    public async Task RequestAsync(IGameSession session, UserAccount me, string username, CancellationToken cancellationToken)
    {
        if (await _userRepository.GetByUsernameAsync(username.Trim(), cancellationToken) is not { } target
            || target.Id == me.Id
            || await _repository.AreFriendsAsync(me.Id, target.Id, cancellationToken))
        {
            return;
        }

        if (await IsListFullAsync(me.Id, cancellationToken))
        {
            session.Send(new MessengerErrorEvent(IncomingOpcodes.FriendList.RequestBuddyMessageComposer, MessengerErrorCode.OwnListFull).Compose());
            return;
        }

        if (await IsListFullAsync(target.Id, cancellationToken))
        {
            session.Send(new MessengerErrorEvent(IncomingOpcodes.FriendList.RequestBuddyMessageComposer, MessengerErrorCode.TheirListFull).Compose());
            return;
        }

        if (!await _repository.AcceptsFriendRequestsAsync(target.Id, cancellationToken))
        {
            session.Send(new MessengerErrorEvent(IncomingOpcodes.FriendList.RequestBuddyMessageComposer, MessengerErrorCode.NotAcceptingRequests).Compose());
            return;
        }

        if (await _repository.HasRequestAsync(target.Id, me.Id, cancellationToken))
        {
            await BefriendAsync(target.Id, me.Id, cancellationToken);
            return;
        }

        if (await _repository.AddRequestAsync(me.Id, target.Id, cancellationToken) && _sessions.TryGetByUserId(target.Id, out ClientSession? targetSession))
        {
            targetSession!.EnqueueSend(new NewBuddyRequestEvent(new FriendRequest(me.Id, me.Username)).Compose());
        }
    }

    public async Task AcceptAsync(IGameSession session, UserAccount me, IReadOnlyCollection<int> requesterIds, CancellationToken cancellationToken)
    {
        Dictionary<int, FriendRequest> pending = (await _repository.GetRequestsAsync(me.Id, cancellationToken)).ToDictionary(r => r.FromUserId);
        var failures = new List<AcceptBuddyFailure>();

        foreach (int requesterId in Capped(requesterIds))
        {
            if (!pending.TryGetValue(requesterId, out FriendRequest? request))
            {
                continue;
            }

            if (await IsListFullAsync(me.Id, cancellationToken))
            {
                failures.Add(new AcceptBuddyFailure(request.FromUsername, MessengerErrorCode.OwnListFull));
            }
            else if (await IsListFullAsync(requesterId, cancellationToken))
            {
                failures.Add(new AcceptBuddyFailure(request.FromUsername, MessengerErrorCode.TheirListFull));
            }
            else
            {
                await BefriendAsync(requesterId, me.Id, cancellationToken);
            }
        }

        if (failures.Count > 0)
        {
            session.Send(new AcceptBuddyResultEvent(failures).Compose());
        }
    }

    public Task DeclineAsync(UserAccount me, bool all, IReadOnlyCollection<int> requesterIds, CancellationToken cancellationToken) =>
        all
            ? _repository.DeclineAllRequestsAsync(me.Id, cancellationToken)
            : _repository.DeclineRequestsAsync(me.Id, Capped(requesterIds), cancellationToken);

    public async Task RemoveAsync(IGameSession session, UserAccount me, IReadOnlyCollection<int> friendIds, CancellationToken cancellationToken)
    {
        IReadOnlyList<int> removed = await _repository.RemoveFriendsAsync(me.Id, Capped(friendIds), cancellationToken);
        if (removed.Count == 0)
        {
            return;
        }

        session.Send(new FriendListUpdateEvent(removed.Select(FriendListChange.Removed).ToList()).Compose());

        byte[] removedMe = new FriendListUpdateEvent([FriendListChange.Removed(me.Id)]).Compose().ToArray();
        foreach (ClientSession friend in OnlineSessionsOf(removed))
        {
            friend.EnqueueSend(removedMe);
        }
    }

    public async Task SearchAsync(IGameSession session, UserAccount me, string query, CancellationToken cancellationToken)
    {
        string prefix = query.Trim();
        IReadOnlyList<FriendSearchHit> hits = prefix.Length == 0
            ? []
            : await _repository.SearchAsync(me.Id, prefix, SearchResultLimit, cancellationToken);

        session.Send(new HabboSearchResultEvent(
            hits.Where(hit => hit.IsFriend).Select(hit => Entry(hit.User)).ToList(),
            hits.Where(hit => !hit.IsFriend).Select(hit => Entry(hit.User)).ToList()).Compose());
    }

    public async Task SendMessageAsync(IGameSession session, UserAccount me, int friendId, string text, CancellationToken cancellationToken)
    {
        string message = Clean(text, MaxMessageLength);
        if (message.Length == 0)
        {
            return;
        }

        if (!session.MessengerFlood.TryPass(Environment.TickCount64, out int secondsLeft, out bool tell))
        {
            if (tell)
            {
                session.Send(new HabboBroadcastMessageEvent(
                    _texts.Format(MessageKeys.MessengerTooFast, ("seconds", secondsLeft))).Compose());
            }

            return;
        }

        if (ModerationService.IsMuted(session))
        {
            session.Send(new InstantMessageErrorEvent(InstantMessageErrorCode.SenderMuted, friendId).Compose());
            return;
        }

        if (!await _repository.AreFriendsAsync(me.Id, friendId, cancellationToken))
        {
            session.Send(new InstantMessageErrorEvent(InstantMessageErrorCode.NotFriend, friendId).Compose());
            return;
        }

        if (!_sessions.TryGetByUserId(friendId, out ClientSession? friend))
        {
            session.Send(new InstantMessageErrorEvent(InstantMessageErrorCode.Offline, friendId).Compose());
            return;
        }

        if (ModerationService.IsMuted(friend!))
        {
            session.Send(new InstantMessageErrorEvent(InstantMessageErrorCode.ReceiverMuted, friendId).Compose());
            return;
        }

        if (friend!.IgnoredUsers.Contains(me.Id))
        {
            return;
        }

        friend.EnqueueSend(new NewConsoleMessageEvent(me.Id, message).Compose());

        _history.Record(new InstantMessageRecord(
            DateTime.UtcNow, me.Id, friendId, RoomTextSanitizer.Sanitize(text.Trim(), MaxMessageLength), session.RemoteAddress));
    }

    public async Task InviteAsync(IGameSession session, UserAccount me, IReadOnlyCollection<int> friendIds, string text, CancellationToken cancellationToken)
    {
        if (session.CurrentRoomId is null)
        {
            return;
        }

        HashSet<int> myFriends = (await _repository.GetFriendIdsAsync(me.Id, cancellationToken)).ToHashSet();
        List<ClientSession> invited = [.. OnlineSessionsOf(Capped(friendIds).Where(myFriends.Contains)).Where(friend => !friend.IgnoredUsers.Contains(me.Id))];
        if (invited.Count == 0 || !session.TryUseCooldown(ActionCooldowns.RoomInvite, ActionCooldowns.Default))
        {
            return;
        }

        byte[] invite = new RoomInviteEvent(me.Id, Clean(text, MaxInviteLength)).Compose().ToArray();
        foreach (ClientSession friend in invited)
        {
            friend.EnqueueSend(invite);
        }
    }

    public async Task FollowAsync(IGameSession session, UserAccount me, int friendId, CancellationToken cancellationToken)
    {
        if (!await _repository.AreFriendsAsync(me.Id, friendId, cancellationToken))
        {
            session.Send(new FollowFriendFailedEvent(FollowFriendError.NotFriend).Compose());
            return;
        }

        if (!_sessions.TryGetByUserId(friendId, out ClientSession? friend))
        {
            session.Send(new FollowFriendFailedEvent(FollowFriendError.Offline).Compose());
            return;
        }

        if (friend!.CurrentRoomId is not { } roomId || !_roomManager.TryGetLoadedRoom(roomId, out Room? room))
        {
            session.Send(new FollowFriendFailedEvent(FollowFriendError.NotInRoom).Compose());
            return;
        }

        if (await _repository.GetProfileAsync(friendId, cancellationToken) is not { AllowsFollowing: true })
        {
            session.Send(new FollowFriendFailedEvent(FollowFriendError.Prevented).Compose());
            return;
        }

        if (session.CurrentRoomId != roomId)
        {
            session.Send(new RoomForwardMessageEvent(isPublic: room!.Data.OwnerId is null, roomId).Compose());
        }
    }

    private async Task BefriendAsync(int requesterId, int userId, CancellationToken cancellationToken)
    {
        if (!await _repository.AcceptRequestAsync(requesterId, userId, cancellationToken))
        {
            return;
        }

        await TellOfNewFriendAsync(userId, requesterId, cancellationToken);
        await TellOfNewFriendAsync(requesterId, userId, cancellationToken);
    }

    private async Task TellOfNewFriendAsync(int userId, int newFriendId, CancellationToken cancellationToken)
    {
        if (_sessions.TryGetByUserId(userId, out ClientSession? session)
            && await _repository.GetProfileAsync(newFriendId, cancellationToken) is { } newFriend)
        {
            session!.EnqueueSend(new FriendListUpdateEvent([FriendListChange.Added(Entry(newFriend))]).Compose());
        }
    }

    private async Task<bool> IsListFullAsync(int userId, CancellationToken cancellationToken)
    {
        int limit = await _subscriptions.IsClubMemberAsync(userId, cancellationToken)
            ? _gameSettings.Current.FriendsLimitClub
            : _gameSettings.Current.FriendsLimit;
        return await _repository.CountFriendsAsync(userId, cancellationToken) >= limit;
    }

    private FriendEntry Entry(MessengerProfile profile) => new(
        profile,
        _sessions.IsOnline(profile.Id),
        profile.LastLoginAt is { } lastLogin ? _clock.ToHotelTime(lastLogin).ToString("dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture) : string.Empty);

    private List<ClientSession> OnlineSessionsOf(IEnumerable<int> userIds)
    {
        var online = new List<ClientSession>();
        foreach (int userId in userIds)
        {
            if (_sessions.TryGetByUserId(userId, out ClientSession? session))
            {
                online.Add(session!);
            }
        }

        return online;
    }

    private string Clean(string text, int maxLength) =>
        RoomTextSanitizer.Sanitize(_profanityFilter.Filter(text.Trim()), maxLength);

    private static IReadOnlyCollection<int> Capped(IReadOnlyCollection<int> ids) => ids.Distinct().Take(MaxIdsPerRequest).ToList();
}
