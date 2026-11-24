using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Incoming.Quest;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;
using DragonEmu.Protocol.Messages.Outgoing.Quest;

namespace DragonEmu.Game.Quests;

public sealed class QuestService
{
    private readonly IQuestRepository _quests;
    private readonly ICurrencyRepository _currency;
    private readonly AchievementService _achievements;

    public QuestService(IQuestRepository quests, ICurrencyRepository currency, AchievementService achievements)
    {
        _achievements = achievements;
        _quests = quests;
        _currency = currency;
    }

    public async Task CompleteIfActiveAsync(IGameSession session, int userId, string classname, CancellationToken cancellationToken)
    {
        string campaign = OpenQuestListMessageComposer.DefaultCampaign;
        IReadOnlyList<QuestDefinition> quests = await _quests.GetCampaignAsync(campaign, userId, cancellationToken);
        if (quests.FirstOrDefault(quest => quest.Accepted && quest.FurniClassname == classname) is not { } quest
            || !await _quests.CompleteAsync(userId, quest.Id, cancellationToken))
        {
            return;
        }

        if (quest.RewardActivityPoints > 0)
        {
            long balance = await _currency.AddBalanceAsync(
                userId, CurrencyType.OfActivityPoints(quest.RewardActivityPointType), quest.RewardActivityPoints, cancellationToken);
            session.Send(new HabboActivityPointNotificationMessageEvent(balance, quest.RewardActivityPoints, quest.RewardActivityPointType).Compose());
        }

        QuestDefinition completed = quest with { Accepted = false, Completed = true };
        session.Send(new QuestCompletedMessageEvent(campaign, completed.ToInfo(DateTime.UtcNow)).Compose());

        await _achievements.IncrementAsync(userId, "ACH_Xm10QuestCompleted", 1, cancellationToken);
    }
}
