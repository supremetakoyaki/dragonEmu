using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Quest;

public sealed class QuestCompletedMessageEvent : IMessageEvent
{
    private readonly string _campaignId;
    private readonly QuestInfo _quest;

    public QuestCompletedMessageEvent(string campaignId, QuestInfo quest)
    {
        _campaignId = campaignId;
        _quest = quest;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Quest.QuestCompletedMessageEvent);
        packet.AppendString(_campaignId);
        QuestMessageData.Append(packet, _quest);
        return packet;
    }
}
