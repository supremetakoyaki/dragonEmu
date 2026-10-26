using DragonEmu.Game.Fuses;
using DragonEmu.Game.Settings;
using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Avatars;
using DragonEmu.Game.Ignoring;
using DragonEmu.Game.Messenger;
using DragonEmu.Game.Moderation;
using DragonEmu.Game.Rooms;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Availability;
using DragonEmu.Protocol.Messages.Outgoing.Handshake;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Protocol.Messages.Incoming.Handshake;

public sealed class SSOTicketMessageComposer : IMessageComposer
{
    private readonly IUserRepository _userRepository;
    private readonly FuseService _fuses;
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly AchievementService _achievementService;
    private readonly HappyHourService _happyHourService;
    private readonly PresenceService _presence;
    private readonly GameSettingsCache _gameSettings;
    private readonly IFavouriteRoomRepository _favouriteRoomRepository;
    private readonly AvatarEffectService _avatarEffectService;
    private readonly ClientSessionManager _sessionManager;
    private readonly MessengerService _messenger;
    private readonly IgnoreService _ignores;
    private readonly ModerationService _moderation;
    private readonly ModerationToolService _moderationTool;
    private readonly HotelClock _clock;
    private readonly UserTagService _tags;
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;
    private readonly ILogger<SSOTicketMessageComposer> _logger;
    private string _ticket = string.Empty;

    private readonly HotelStatus _hotel;
    private readonly MessageTexts _texts;

    public SSOTicketMessageComposer(
        IUserRepository userRepository, FuseService fuses, IUserSettingsRepository userSettingsRepository,
        AchievementService achievementService, HappyHourService happyHourService, PresenceService presence, GameSettingsCache gameSettings,
        IFavouriteRoomRepository favouriteRoomRepository, AvatarEffectService avatarEffectService, ClientSessionManager sessionManager,
        MessengerService messenger, IgnoreService ignores, ModerationService moderation, ModerationToolService moderationTool,
        HotelClock clock, UserTagService tags, IRoomManager roomManager, RoomBroadcaster broadcaster, HotelStatus hotel, ILogger<SSOTicketMessageComposer> logger, MessageTexts texts)
    {
        _texts = texts;
        _hotel = hotel;
        _roomManager = roomManager;
        _broadcaster = broadcaster;
        _clock = clock;
        _tags = tags;
        _ignores = ignores;
        _moderation = moderation;
        _moderationTool = moderationTool;
        _sessionManager = sessionManager;
        _messenger = messenger;
        _userRepository = userRepository;
        _fuses = fuses;
        _userSettingsRepository = userSettingsRepository;
        _achievementService = achievementService;
        _happyHourService = happyHourService;
        _presence = presence;
        _gameSettings = gameSettings;
        _favouriteRoomRepository = favouriteRoomRepository;
        _avatarEffectService = avatarEffectService;
        _logger = logger;
    }

    public void Parse(ClientPacket packet)
    {
        _ticket = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not null)
        {
            _logger.LogWarning("Session {SessionId} sent a second SSO login attempt!", session.Id);
            return;
        }

        if (_hotel.IsShuttingDown)
        {
            (int hour, int minute) = _hotel.ReopensAt;
            _logger.LogInformation("Session {SessionId} tried to log in while the hotel was shutting down.", session.Id);
            session.Send(new LoginFailedHotelClosedMessageEvent(hour, minute).Compose());
            _ = ModerationService.DisconnectLaterAsync(session);
            return;
        }

        (DateTime todayStart, DateTime yesterdayStart) = _clock.DayStartsUtc();
        (UserAccount Account, int LoginStreakDays)? result = await _userRepository.ValidateAndConsumeSsoTokenAsync(
            _ticket, session.RemoteAddress, todayStart, yesterdayStart, cancellationToken);

        if (result is null)
        {
            _logger.LogWarning("Session {SessionId} presented an invalid or expired SSO ticket.", session.Id);
            return;
        }

        (UserAccount account, int loginStreakDays) = result.Value;

        if (await _moderation.GetActiveBanAsync(account.Id, cancellationToken) is { } ban)
        {
            _logger.LogInformation("Session {SessionId}: {Username} is banned until {Until:u}.", session.Id, account.Username, ban.ExpiresAt);
            ModerationService.ShowBanAndDisconnect(session, ban.Reason);
            return;
        }

        session.Authenticate(account);
        await _ignores.LoadAsync(session, account.Id, cancellationToken);
        await _moderation.LoadMuteAsync(session, account.Id, cancellationToken);
        foreach (ClientSession older in _sessionManager.SetLoggedIn(session.Id))
        {
            LogOut(older);
        }

        _logger.LogInformation(
            "Session {SessionId} authenticated as {Username} (id {UserId}).",
            session.Id, account.Username, account.Id);

        await _messenger.AnnounceAsync(account.Id, cancellationToken);

        await _achievementService.SetAsync(account.Id, "ACH_Login", loginStreakDays, cancellationToken);
        int daysRegistered = (int)(DateTime.UtcNow - account.CreatedAt).TotalDays;
        await _achievementService.SetAsync(account.Id, "ACH_RegistrationDuration", daysRegistered, cancellationToken);
        (_, int clubMonthsAccumulated, _) = await _userRepository.GetClubStatusAsync(account.Id, cancellationToken);
        await _achievementService.SetAsync(account.Id, "HC", clubMonthsAccumulated, cancellationToken);

        await _tags.CheckAchievementAsync(account.Id, cancellationToken);

        if (_happyHourService.IsHappyHourNow())
        {
            await _achievementService.SetAsync(account.Id, "ACH_HappyHour", 1, cancellationToken);
        }

        await _presence.OnLoginAsync(account.Id, cancellationToken);

        await _fuses.SendHeldAsync(session, account, cancellationToken);
        await _avatarEffectService.SendInventoryAsync(session, account.Id, cancellationToken);
        session.Send(new AvailabilityStatusMessageEvent().Compose());
        session.Send(new AuthenticationOKMessageEvent().Compose());
        session.Send(new InfoFeedEnableMessageEvent().Compose());

        (_, int homeRoom) = await _userSettingsRepository.GetOrCreateAsync(account.Id, cancellationToken);
        session.Send(new NavigatorSettingsEvent(homeRoom).Compose());

        int favouritesLimit = _gameSettings.Current.MaxFavouriteRooms;
        IReadOnlyList<int> favouriteRoomIds = await _favouriteRoomRepository.GetRoomIdsAsync(account.Id, cancellationToken);
        session.Send(new FavouritesEvent(favouritesLimit, favouriteRoomIds).Compose());

        await _moderationTool.SendInitAsync(session, account, cancellationToken);
    }

    private void LogOut(ClientSession older)
    {
        _logger.LogInformation("Session {SessionId}: {Username} was already logged in in another session. I will log them out there...",
            older.Id, older.AuthenticatedUser?.Username);

        if (older.CurrentRoomId is { } roomId)
        {
            RoomExit.ToHotelView(older, _roomManager.TryGetLoadedRoom(roomId, out Room? room) ? room : null, _broadcaster);
        }

        older.EnqueueSend(new ModMessageEvent(_texts.Get(MessageKeys.LoggedInElsewhere)).Compose());
        _ = ModerationService.DisconnectLaterAsync(older);
    }
}
