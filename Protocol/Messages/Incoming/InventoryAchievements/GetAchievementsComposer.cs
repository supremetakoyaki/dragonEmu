using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.InventoryAchievements;

namespace DragonEmu.Protocol.Messages.Incoming.InventoryAchievements;

public sealed class GetAchievementsComposer : IMessageComposer
{
    private readonly AchievementDefinitionCache _definitionCache;
    private readonly IUserAchievementRepository _userAchievementRepository;

    public GetAchievementsComposer(AchievementDefinitionCache definitionCache, IUserAchievementRepository userAchievementRepository)
    {
        _definitionCache = definitionCache;
        _userAchievementRepository = userAchievementRepository;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        var entries = new List<(int Type, int Level, string Code)>();
        foreach (AchievementDefinition definition in _definitionCache.Definitions)
        {
            if (!definition.Enabled || definition.Levels.Count == 0)
            {
                continue;
            }

            AchievementProgress progress = await _userAchievementRepository.GetProgressAsync(account.Id, definition.Id, cancellationToken);
            int nextLevel = progress.CurrentLevel + 1;
            if (nextLevel > definition.MaxLevel)
            {
                continue;
            }

            AchievementLevel? level = definition.Levels.FirstOrDefault(l => l.Level == nextLevel);
            if (level is null)
            {
                continue;
            }

            entries.Add((definition.Id, nextLevel, level.BadgeCode));
        }

        session.Send(new AchievementsEvent(entries).Compose());
    }
}
