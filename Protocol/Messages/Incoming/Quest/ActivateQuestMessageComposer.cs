using DragonEmu.Domain;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Quest;

namespace DragonEmu.Protocol.Messages.Incoming.Quest;

public sealed class ActivateQuestMessageComposer : IMessageComposer
{
    private readonly IQuestRepository _quests;
    private int _questId;

    public ActivateQuestMessageComposer(IQuestRepository quests)
    {
        _quests = quests;
    }

    public void Parse(ClientPacket packet)
    {
        _questId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        string campaign = OpenQuestListMessageComposer.DefaultCampaign;
        DateTime now = DateTime.UtcNow;
        IReadOnlyList<QuestDefinition> quests = await _quests.GetCampaignAsync(campaign, account.Id, cancellationToken);
        if (quests.FirstOrDefault(quest => quest.Id == _questId) is not { } quest || quest.StatusAt(now) != QuestStatus.Available)
        {
            return;
        }

        await _quests.AcceptAsync(account.Id, quest.Id, cancellationToken);
        session.Send(new QuestAcceptedMessageEvent(campaign, (quest with { Accepted = true }).ToInfo(now)).Compose());
    }
}
