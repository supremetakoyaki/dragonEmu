using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Avatars;
using DragonEmu.Game.Rooms;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Furniture;

public sealed class IceTagService
{
    private readonly ClientSessionManager _sessions;
    private readonly AchievementService _achievements;
    private readonly IAchievementItemRepository _achievementItems;
    private readonly ILogger<IceTagService> _logger;

    public IceTagService(ClientSessionManager sessions, AchievementService achievements, IAchievementItemRepository achievementItems, ILogger<IceTagService> logger)
    {
        _sessions = sessions;
        _achievements = achievements;
        _achievementItems = achievementItems;
        _logger = logger;
    }

    public void HandleTick(IceTagTickResult result)
    {
        foreach (long sessionId in result.StartedSkating)
        {
            if (_sessions.TryGet(sessionId, out ClientSession? session))
            {
                session!.AvatarEffectId = AvatarEffectService.NoEffect;
            }
        }

        if (result.Caught.Count == 0 && result.MinutesSkated.Count == 0)
        {
            return;
        }

        _ = CountAsync(result).ContinueWith(t => _logger.LogError(t.Exception, "Ice Tag's achievements failed."), TaskContinuationOptions.OnlyOnFaulted);
    }

    private async Task CountAsync(IceTagTickResult result)
    {
        foreach (int caught in result.Caught)
        {
            await _achievements.IncrementAsync(caught, IceTagRules.CaughtAchievement);
        }

        foreach ((int accountId, int minutes) in result.MinutesSkated)
        {
            await _achievements.IncrementAsync(accountId, IceTagRules.TimeOnIceAchievement, minutes);
        }
    }

    public async Task PatchPlacedAsync(int userId, int itemId, CancellationToken cancellationToken)
    {
        if (await _achievementItems.TryCountAsync(userId, IceTagRules.RinkBuilderAchievement, itemId, cancellationToken))
        {
            await _achievements.IncrementAsync(userId, IceTagRules.RinkBuilderAchievement, 1, cancellationToken);
        }
    }
}
