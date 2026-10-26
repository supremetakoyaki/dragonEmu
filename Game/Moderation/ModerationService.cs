using DragonEmu.Domain;
using DragonEmu.Game.Fuses;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Settings;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;

namespace DragonEmu.Game.Moderation;

public sealed class ModerationService
{
    public static readonly TimeSpan DisconnectDelay = TimeSpan.FromMilliseconds(6 * 500);

    public const string ModeratorAccessFuse = "fuse_moderator_access";

    public const int PermanentHours = 100000;

    public static readonly DateTime Permanent = new(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);

    private const int MaxHours = 24 * 365 * 10;
    private const int MaxReasonLength = 255;

    private readonly IModerationRepository _repository;
    private readonly IUserRepository _users;
    private readonly ClientSessionManager _sessions;
    private readonly GameSettingsCache _gameSettings;
    private readonly FuseService _fuses;
    private readonly MessageTexts _texts;

    public ModerationService(
        IModerationRepository repository, IUserRepository users, ClientSessionManager sessions, GameSettingsCache gameSettings, FuseService fuses, MessageTexts texts)
    {
        _texts = texts;
        _repository = repository;
        _users = users;
        _sessions = sessions;
        _gameSettings = gameSettings;
        _fuses = fuses;
    }

    public async Task<string> BanAsync(UserAccount issuer, string username, int? amount, string? reason, bool superban, CancellationToken cancellationToken)
    {
        GameSettings settings = _gameSettings.Current;
        if (await LowerRankedAsync(issuer, username, cancellationToken) is not { } target)
        {
            return await RefusalAsync(issuer, username, "ban", cancellationToken);
        }

        long asked = superban ? (long)(amount ?? settings.SuperbanDefaultDays) * 24 : amount ?? settings.BanDefaultHours;
        int hours = (int)Math.Clamp(asked, 1, PermanentHours);
        string why = await BanCheckedAsync(issuer, target, hours, reason, superban, cancellationToken);
        string length = hours >= PermanentHours ? _texts.Get(MessageKeys.BanLengthPermanent) : _texts.Format(MessageKeys.BanLengthFor, ("duration", Duration(hours)));
        return superban ? _texts.Format(MessageKeys.SuperbanApplied, ("username", target.Username), ("length", length), ("reason", why)) : _texts.Format(MessageKeys.BanApplied, ("username", target.Username), ("length", length), ("reason", why));
    }

    public async Task<bool> BanAsync(UserAccount issuer, UserAccount target, int hours, string? reason, CancellationToken cancellationToken)
    {
        if (target.Rank >= issuer.Rank)
        {
            return false;
        }

        await BanCheckedAsync(issuer, target, Math.Clamp(hours, 1, PermanentHours), reason, superban: false, cancellationToken);
        return true;
    }

    private async Task<string> BanCheckedAsync(UserAccount issuer, UserAccount target, int hours, string? reason, bool superban, CancellationToken cancellationToken)
    {
        string why = string.IsNullOrWhiteSpace(reason) ? _gameSettings.Current.BanDefaultReason : reason.Trim();
        why = why[..Math.Min(why.Length, MaxReasonLength)];

        DateTime expiresAt = hours >= PermanentHours ? Permanent : DateTime.UtcNow.ToWholeSecond().AddHours(hours);
        await _repository.BanAsync(target.Id, issuer.Id, superban, why, expiresAt, cancellationToken);
        if (_sessions.TryGetByUserId(target.Id, out ClientSession? online))
        {
            ShowBanAndDisconnect(online!, why);
        }

        return why;
    }

    public Task<UserBan?> GetActiveBanAsync(int userId, CancellationToken cancellationToken) => _repository.GetActiveBanAsync(userId, cancellationToken);

    public static void ShowBanAndDisconnect(IGameSession session, string reason)
    {
        session.Send(new UserBannedMessageEvent(reason).Compose());
        _ = DisconnectLaterAsync(session);
    }

    public async Task LoadMuteAsync(IGameSession session, int userId, CancellationToken cancellationToken) =>
        session.MutedUntil = await _repository.GetMuteAsync(userId, cancellationToken);

    public static bool IsMuted(IGameSession session) => session.MutedUntil > DateTime.UtcNow;

    public async Task<string> MuteAsync(UserAccount issuer, string username, int? hours, CancellationToken cancellationToken)
    {
        if (await LowerRankedAsync(issuer, username, cancellationToken) is not { } target)
        {
            return await RefusalAsync(issuer, username, "mute", cancellationToken);
        }

        int length = Math.Clamp(hours ?? _gameSettings.Current.MuteDefaultHours, 1, MaxHours);
        DateTime until = DateTime.UtcNow.AddHours(length);
        await _repository.MuteAsync(target.Id, issuer.Id, until, cancellationToken);
        if (_sessions.TryGetByUserId(target.Id, out ClientSession? online))
        {
            online!.MutedUntil = until;
        }

        return _texts.Format(MessageKeys.MuteApplied, ("username", target.Username), ("duration", Duration(length)));
    }

    public async Task<string> UnmuteAsync(string username, CancellationToken cancellationToken)
    {
        if (await _users.GetByUsernameAsync(username, cancellationToken) is not { } target)
        {
            return _texts.Format(MessageKeys.UserNotFound, ("username", username));
        }

        bool wasMuted = await _repository.UnmuteAsync(target.Id, cancellationToken);
        if (_sessions.TryGetByUserId(target.Id, out ClientSession? online))
        {
            online!.MutedUntil = null;
        }

        return wasMuted ? _texts.Format(MessageKeys.UnmuteApplied, ("username", target.Username)) : _texts.Format(MessageKeys.NotMuted, ("username", target.Username));
    }

    public async Task<string> MuteRoomAsync(UserAccount issuer, Room room, int? hours, CancellationToken cancellationToken)
    {
        int length = Math.Clamp(hours ?? _gameSettings.Current.RoomMuteDefaultHours, 1, MaxHours);
        DateTime until = DateTime.UtcNow.AddHours(length);
        await _repository.MuteRoomAsync(room.Id, issuer.Id, until, cancellationToken);
        room.MutedUntil = until;
        return _texts.Format(MessageKeys.RoomMuteApplied, ("duration", Duration(length)));
    }

    public async Task<string> UnmuteRoomAsync(Room room, CancellationToken cancellationToken)
    {
        bool wasMuted = await _repository.UnmuteRoomAsync(room.Id, cancellationToken);
        room.MutedUntil = null;
        return wasMuted ? _texts.Get(MessageKeys.RoomUnmuteApplied) : _texts.Get(MessageKeys.RoomNotMuted);
    }

    public async Task<(bool Blocked, string? Alert)> WhyCantChatAsync(IGameSession session, UserAccount account, Room room, CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        if (session.MutedUntil is { } until && until > now)
        {
            return (true, _texts.Format(MessageKeys.YouAreMuted, ("duration", Remaining(until - now))));
        }

        if (room.MutedUntil > now && !await _fuses.HoldsAsync(account, ModeratorAccessFuse, cancellationToken: cancellationToken))
        {
            return (true, null);
        }

        return (false, null);
    }

    private async Task<UserAccount?> LowerRankedAsync(UserAccount issuer, string username, CancellationToken cancellationToken) =>
        await _users.GetByUsernameAsync(username, cancellationToken) is { } target && target.Rank < issuer.Rank ? target : null;

    private async Task<string> RefusalAsync(UserAccount issuer, string username, string verb, CancellationToken cancellationToken) =>
        await _users.GetByUsernameAsync(username, cancellationToken) is { } target
            ? (target.Id == issuer.Id
                ? _texts.Format(MessageKeys.RankRefusalSelf, ("verb", verb), ("username", target.Username))
                : _texts.Format(MessageKeys.RankRefusalLowerOnly, ("verb", verb), ("username", target.Username)))
            : _texts.Format(MessageKeys.UserNotFound, ("username", username));

    public static async Task DisconnectLaterAsync(IGameSession session)
    {
        await Task.Delay(DisconnectDelay);
        session.Close();
    }

    private string Duration(int hours) =>
        hours % 24 == 0 && hours >= 24 ? _texts.Duration(DurationUnit.Day, hours / 24) : _texts.Duration(DurationUnit.Hour, hours);

    private string Remaining(TimeSpan left) =>
        left.TotalMinutes >= 120
            ? _texts.Duration(DurationUnit.Hour, (int)Math.Ceiling(left.TotalHours))
            : _texts.Duration(DurationUnit.Minute, Math.Max(1, (int)Math.Ceiling(left.TotalMinutes)));
}
