using DragonEmu.Domain;
using DragonEmu.Game.Fuses;
using DragonEmu.Game.Rooms;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol;
using DragonEmu.Protocol.Messages.Outgoing.InventoryBadges;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;
using DragonEmu.Protocol.Messages.Outgoing.Users;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Achievements;

public sealed class AchievementService
{
    private readonly AchievementDefinitionCache _definitionCache;
    private readonly IUserAchievementRepository _userAchievementRepository;
    private readonly IUserBadgeRepository _userBadgeRepository;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly ClientSessionManager _sessionManager;
    private readonly FuseService _fuses;
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly ILogger<AchievementService> _logger;

    public AchievementService(
        AchievementDefinitionCache definitionCache,
        IUserAchievementRepository userAchievementRepository,
        IUserBadgeRepository userBadgeRepository,
        ICurrencyRepository currencyRepository,
        ClientSessionManager sessionManager,
        FuseService fuses,
        IRoomManager roomManager,
        RoomBroadcaster roomBroadcaster,
        ILogger<AchievementService> logger)
    {
        _fuses = fuses;
        _roomManager = roomManager;
        _roomBroadcaster = roomBroadcaster;
        _definitionCache = definitionCache;
        _userAchievementRepository = userAchievementRepository;
        _userBadgeRepository = userBadgeRepository;
        _currencyRepository = currencyRepository;
        _sessionManager = sessionManager;
        _logger = logger;
    }

    public Task IncrementAsync(int userId, string codeBase, int delta = 1, CancellationToken cancellationToken = default) =>
        ApplyAsync(userId, codeBase, counter => counter + delta, cancellationToken);

    public Task SetAsync(int userId, string codeBase, int value, CancellationToken cancellationToken = default) =>
        ApplyAsync(userId, codeBase, _ => value, cancellationToken);

    public Task RaiseToAsync(int userId, string codeBase, int value, CancellationToken cancellationToken = default) =>
        ApplyAsync(userId, codeBase, counter => Math.Max(counter, value), cancellationToken);

    public Task IncrementFromAsync(int userId, string codeBase, int from, int delta = 1, CancellationToken cancellationToken = default) =>
        ApplyAsync(userId, codeBase, counter => counter >= from ? counter + delta : counter, cancellationToken);

    private async Task ApplyAsync(int userId, string codeBase, Func<int, int> computeNewCounter, CancellationToken cancellationToken)
    {
        AchievementDefinition? definition = _definitionCache.GetByCodeBase(codeBase);
        if (definition is null || !definition.Enabled || definition.Levels.Count == 0)
        {
            return;
        }

        AchievementProgress progress = await _userAchievementRepository.GetProgressAsync(userId, definition.Id, cancellationToken);
        int newCounter = computeNewCounter(progress.Counter);
        if (newCounter == progress.Counter)
        {
            return;
        }

        int newLevel = progress.CurrentLevel;
        foreach (AchievementLevel level in definition.Levels)
        {
            if (level.Level > newLevel && newCounter >= level.Threshold)
            {
                newLevel = level.Level;
            }
        }

        await _userAchievementRepository.SetProgressAsync(userId, definition.Id, newCounter, newLevel, cancellationToken);

        if (newLevel <= progress.CurrentLevel)
        {
            return;
        }

        _sessionManager.TryGetByUserId(userId, out ClientSession? session);

        for (int level = progress.CurrentLevel + 1; level <= newLevel; level++)
        {
            AchievementLevel awarded = definition.Levels.First(l => l.Level == level);
            if (awarded.RewardActivityPoints > 0)
            {
                long newBalance = await _currencyRepository.AddBalanceAsync(
                    userId, CurrencyType.ActivityPoints, awarded.RewardActivityPoints, cancellationToken);
                session?.EnqueueSend(new HabboActivityPointNotificationMessageEvent(newBalance, awarded.RewardActivityPoints).Compose());
            }
        }

        string? previousBadgeCode = progress.CurrentLevel > 0
            ? definition.Levels.First(l => l.Level == progress.CurrentLevel).BadgeCode
            : null;
        AchievementLevel topLevel = definition.Levels.First(l => l.Level == newLevel);

        int? wornSlot = null;
        if (previousBadgeCode is not null)
        {
            wornSlot = await _userBadgeRepository.ReplaceAsync(userId, previousBadgeCode, topLevel.BadgeCode, cancellationToken);
        }
        else
        {
            await _userBadgeRepository.AwardAsync(userId, topLevel.BadgeCode, cancellationToken);
        }

        _logger.LogDebug("User {UserId} reached {CodeBase} level {Level}.", userId, codeBase, newLevel);

        session?.EnqueueSend(new HabboAchievementNotificationMessageEvent(
            definition.Id, newLevel, topLevel.BadgeCode, previousBadgeCode ?? string.Empty).Compose());

        if (wornSlot is not null && session is not null)
        {
            await ShowWornBadgesAsync(session, userId, cancellationToken);
        }

        if (session?.AuthenticatedUser is { } account && _fuses.LooksAtAchievement(codeBase))
        {
            await _fuses.SendHeldAsync(session, account, cancellationToken);
        }
    }

    private async Task ShowWornBadgesAsync(ClientSession session, int userId, CancellationToken cancellationToken)
    {
        session.EnqueueSend(BadgesEvent.For(await _userBadgeRepository.GetAllAsync(userId, cancellationToken)).Compose());

        ServerPacket worn = new HabboUserBadgesMessageEvent(userId, await _userBadgeRepository.GetWornAsync(userId, cancellationToken)).Compose();
        if (session.CurrentRoomId is { } roomId && _roomManager.TryGetLoadedRoom(roomId, out Room? room))
        {
            _roomBroadcaster.Broadcast(room!, worn);
        }
        else
        {
            session.EnqueueSend(worn);
        }
    }
}
