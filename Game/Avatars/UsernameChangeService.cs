using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Messenger;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Settings;
using DragonEmu.Persistence;
using DragonEmu.Protocol;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Protocol.Messages.Outgoing.Users;

namespace DragonEmu.Game.Avatars;

public sealed record UsernameVerdict(UsernameCheck Check, string Name, IReadOnlyList<string> Suggestions);

public sealed class UsernameChangeService
{
    public static readonly TimeSpan MinimumCooldown = TimeSpan.FromHours(1);

    private const int MaxSuggestions = 3;
    private const int SuggestionCandidates = 12;

    private readonly IUserRepository _userRepository;
    private readonly UsernameRules _rules;
    private readonly GameSettingsCache _gameSettings;
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;
    private readonly AchievementService _achievementService;
    private readonly MessengerService _messenger;
    private readonly TimeProvider _time;
    private readonly MessageTexts _texts;

    public UsernameChangeService(
        IUserRepository userRepository, UsernameRules rules, GameSettingsCache gameSettings, IRoomManager roomManager, RoomBroadcaster broadcaster,
        AchievementService achievementService, MessengerService messenger, MessageTexts texts, TimeProvider? time = null)
    {
        _texts = texts;
        _userRepository = userRepository;
        _rules = rules;
        _gameSettings = gameSettings;
        _roomManager = roomManager;
        _broadcaster = broadcaster;
        _achievementService = achievementService;
        _messenger = messenger;
        _time = time ?? TimeProvider.System;
    }

    public static TimeSpan Cooldown(UsernameSettings settings) =>
        settings.CooldownDays <= 0 ? MinimumCooldown : TimeSpan.FromDays(settings.CooldownDays);

    public static bool MayChange(UsernameSettings settings, UsernameChangeStatus status, DateTime now) =>
        settings.ChangeAllowed && status.CanChange && Remaining(settings, status) != 0
        && (status.ChangedAt is not { } last || now - last >= Cooldown(settings));

    public static int Remaining(UsernameSettings settings, UsernameChangeStatus status) =>
        settings.ChangeLimit < 0 ? -1 : Math.Max(settings.ChangeLimit - status.Changes, 0);

    public static string Describe(MessageTexts texts, UsernameSettings settings, UsernameChangeStatus status, DateTime now)
    {
        string changes = settings.ChangeLimit < 0
            ? texts.Format(MessageKeys.NameChangesUnlimited, ("changes", status.Changes))
            : texts.Format(MessageKeys.NameChangesUsed,
                ("used", Math.Min(status.Changes, settings.ChangeLimit)), ("limit", settings.ChangeLimit), ("left", Remaining(settings, status)));

        TimeSpan cooldown = Cooldown(settings);
        string every = settings.CooldownDays <= 0
            ? texts.Get(MessageKeys.NameChangeEveryHour)
            : texts.Format(MessageKeys.NameChangeEveryDays, ("days", settings.CooldownDays));
        string wait = status.ChangedAt is { } last && now - last < cooldown && Remaining(settings, status) != 0
            ? texts.Format(MessageKeys.NameChangeWait, ("wait", FormatWait(texts, cooldown - (now - last))))
            : string.Empty;
        return texts.Format(MessageKeys.NameChangeSummary, ("changes", changes), ("every", every), ("wait", wait));
    }

    private static string FormatWait(MessageTexts texts, TimeSpan wait) =>
        wait.TotalDays >= 1 ? texts.Duration(DurationUnit.Day, (int)Math.Ceiling(wait.TotalDays))
        : wait.TotalHours >= 1 ? texts.Duration(DurationUnit.Hour, (int)Math.Ceiling(wait.TotalHours))
        : texts.Duration(DurationUnit.Minute, (int)Math.Ceiling(wait.TotalMinutes));

    public async Task<ServerPacket> DescribeAsync(UserAccount account, CancellationToken cancellationToken) =>
        new ModMessageEvent(Describe(
            _texts, _gameSettings.Current.Username, await _userRepository.GetUsernameChangeStatusAsync(account.Id, cancellationToken), Now)).Compose();

    public async Task<bool> MayChangeAsync(UserAccount account, CancellationToken cancellationToken) =>
        MayChange(_gameSettings.Current.Username, await _userRepository.GetUsernameChangeStatusAsync(account.Id, cancellationToken), Now);

    public async Task<UsernameVerdict> CheckAsync(UserAccount account, string requested, CancellationToken cancellationToken)
    {
        string name = UsernameRules.Normalize(requested);
        if (!await MayChangeAsync(account, cancellationToken))
        {
            return new UsernameVerdict(UsernameCheck.ChangeNotAllowed, name, []);
        }

        UsernameCheck check = _rules.Check(name);
        if (check == UsernameCheck.Ok && await IsTakenAsync(account, name, cancellationToken))
        {
            check = UsernameCheck.Taken;
        }

        return new UsernameVerdict(check, name, check == UsernameCheck.Ok ? [] : await SuggestAsync(account, name, cancellationToken));
    }

    public async Task<UsernameVerdict> ChangeAsync(IGameSession session, UserAccount account, string requested, CancellationToken cancellationToken)
    {
        UsernameVerdict verdict = await CheckAsync(account, requested, cancellationToken);
        if (verdict.Check != UsernameCheck.Ok)
        {
            return verdict;
        }

        if (!await _userRepository.TryChangeUsernameAsync(account.Id, account.Username, verdict.Name, cancellationToken))
        {
            return verdict with { Check = UsernameCheck.Taken, Suggestions = await SuggestAsync(account, verdict.Name, cancellationToken) };
        }

        await _achievementService.IncrementAsync(account.Id, "ACH_Name", 1, cancellationToken);

        UserAccount renamed = account with { Username = verdict.Name };
        session.Authenticate(renamed);
        ShowEverywhere(session, renamed);
        await _messenger.AnnounceAsync(account.Id, cancellationToken);
        session.NameChangeDialogOpen = false;
        session.Send(await DescribeAsync(renamed, cancellationToken));

        return verdict;
    }

    private DateTime Now => _time.GetUtcNow().UtcDateTime;

    private async Task<bool> IsTakenAsync(UserAccount account, string name, CancellationToken cancellationToken) =>
        await _userRepository.GetByUsernameAsync(name, cancellationToken) is { } other && other.Id != account.Id;

    private async Task<IReadOnlyList<string>> SuggestAsync(UserAccount account, string name, CancellationToken cancellationToken)
    {
        int maxLength = _gameSettings.Current.Username.MaxLength;
        string stem = new([.. name.Where(c => _rules.AllowsCharacters(c.ToString()))]);
        stem = stem.Length <= maxLength ? stem : stem[..maxLength];
        if (stem.Length == 0)
        {
            return [];
        }

        var candidates = new List<string> { stem };
        for (int number = 1; candidates.Count < SuggestionCandidates; number++)
        {
            string digits = number.ToString();
            candidates.Add((stem.Length + digits.Length <= maxLength ? stem : stem[..Math.Max(maxLength - digits.Length, 0)]) + digits);
        }

        var suggestions = new List<string>();
        foreach (string candidate in candidates)
        {
            if (suggestions.Count == MaxSuggestions)
            {
                break;
            }

            if (!string.Equals(candidate, name, StringComparison.OrdinalIgnoreCase)
                && !suggestions.Contains(candidate, StringComparer.OrdinalIgnoreCase)
                && _rules.Check(candidate) == UsernameCheck.Ok
                && !await IsTakenAsync(account, candidate, cancellationToken))
            {
                suggestions.Add(candidate);
            }
        }

        return suggestions;
    }

    private void ShowEverywhere(IGameSession session, UserAccount renamed)
    {
        int virtualId = 0;
        Room? inRoom = null;

        if (session.CurrentRoomId is { } roomId
            && _roomManager.TryGetLoadedRoom(roomId, out Room? room)
            && room!.TryGetActor(session.Id, out RoomActor? actor))
        {
            lock (actor!.Lock)
            {
                actor.Username = renamed.Username;
            }

            virtualId = actor.VirtualId;
            inRoom = room;

            _broadcaster.Broadcast(room, new UsersMessageEvent([actor]).Compose());
        }

        ServerPacket changed = new UserNameChangedMessageEvent(renamed.Id, virtualId, renamed.Username).Compose();
        if (inRoom is not null)
        {
            _broadcaster.Broadcast(inRoom, changed);
        }
        else
        {
            session.Send(changed);
        }

        foreach (Room loaded in _roomManager.LoadedRooms)
        {
            if (loaded.Data.OwnerId == renamed.Id)
            {
                loaded.ApplySettingsUpdate(loaded.Data with { OwnerUsername = renamed.Username });
            }

            foreach (RoomActor pet in loaded.Actors)
            {
                if (pet.Pet is { } brain && brain.OwnerId == renamed.Id)
                {
                    brain.OwnerName = renamed.Username;
                }
            }
        }
    }
}
