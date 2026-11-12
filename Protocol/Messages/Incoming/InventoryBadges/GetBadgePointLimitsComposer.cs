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

        List<(string BadgeCode, int Limit)> limits = _definitions.Definitions
            .Where(definition => definition.Enabled)
            .SelectMany(definition => definition.Levels)
            .Select(level => (level.BadgeCode, level.Threshold))
            .ToList();

        session.Send(new BadgePointLimitsEvent(limits).Compose());
        return ValueTask.CompletedTask;
    }
}
