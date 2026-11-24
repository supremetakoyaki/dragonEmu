using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Quest;

public sealed class QuestAcceptedMessageEvent : IMessageEvent
{
    private readonly string _campaignId;
    private readonly QuestInfo _quest;

    public QuestAcceptedMessageEvent(string campaignId, QuestInfo quest)
    {
        _campaignId = campaignId;
        _quest = quest;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Quest.QuestAcceptedMessageEvent);
        packet.AppendString(_campaignId);
        QuestMessageData.Append(packet, _quest);
        return packet;
    }
}
