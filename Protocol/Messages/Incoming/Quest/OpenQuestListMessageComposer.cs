using DragonEmu.Domain;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Quest;

namespace DragonEmu.Protocol.Messages.Incoming.Quest;

public sealed class OpenQuestListMessageComposer : IMessageComposer
{
    public const string DefaultCampaign = "xmas10";

    private readonly IQuestRepository _quests;

    public OpenQuestListMessageComposer(IQuestRepository quests)
    {
        _quests = quests;
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

        IReadOnlyList<QuestDefinition> quests = await _quests.GetCampaignAsync(DefaultCampaign, account.Id, cancellationToken);
        DateTime now = DateTime.UtcNow;
        session.Send(new QuestListMessageEvent(DefaultCampaign, quests.Select(quest => quest.ToInfo(now)).ToList()).Compose());
    }
}
