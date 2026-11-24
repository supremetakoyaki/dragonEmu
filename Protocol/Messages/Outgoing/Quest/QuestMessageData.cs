using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Quest;

internal static class QuestMessageData
{
    public static void Append(ServerPacket packet, QuestInfo quest)
    {
        packet.AppendInt32(quest.Id);
        packet.AppendInt32(quest.Status);
        packet.AppendString(quest.Type);
        packet.AppendString(quest.RoomItemTypeName);
        packet.AppendInt32(quest.RewardCurrencyAmount);
        packet.AppendString(quest.EndDateTimeStamp);
        packet.AppendString(quest.LocalizationCode);
    }
}
