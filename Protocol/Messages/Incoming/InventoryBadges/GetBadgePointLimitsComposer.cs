using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Protocol.Messages.Outgoing.InventoryBadges;

namespace DragonEmu.Protocol.Messages.Incoming.InventoryBadges;

public sealed class GetBadgePointLimitsComposer : IMessageComposer
{
    private readonly AchievementDefinitionCache _definitions;

    public GetBadgePointLimitsComposer(AchievementDefinitionCache definitions)
    {
        _definitions = definitions;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is null)
        {
            return ValueTask.CompletedTask;
        }

        const string Prefix = "ACH_";
        List<(string Name, IReadOnlyList<(int Level, int Limit)> Levels)> limits = _definitions.Definitions
            .Where(definition => definition.Enabled && definition.CodeBase.StartsWith(Prefix, StringComparison.Ordinal))
            .Select(definition => (
                Name: definition.CodeBase[Prefix.Length..],
                Levels: (IReadOnlyList<(int Level, int Limit)>)definition.Levels
                    .Where(level => level.BadgeCode == definition.CodeBase + level.Level)
                    .Select(level => (level.Level, level.Threshold))
                    .ToList()))
            .Where(achievement => achievement.Levels.Count > 0)
            .ToList();

        session.Send(new BadgePointLimitsEvent(limits).Compose());
        return ValueTask.CompletedTask;
    }
}
