using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Quest;

public sealed class QuestListMessageEvent : IMessageEvent
{
    private readonly string _campaignId;
    private readonly IReadOnlyList<QuestInfo> _quests;

    public QuestListMessageEvent(string campaignId, IReadOnlyList<QuestInfo> quests)
    {
        _campaignId = campaignId;
        _quests = quests;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Quest.QuestListMessageEvent);
        packet.AppendString(_campaignId);
        packet.AppendInt32(_quests.Count);
        foreach (QuestInfo quest in _quests)
        {
            QuestMessageData.Append(packet, quest);
        }

        return packet;
    }
}
