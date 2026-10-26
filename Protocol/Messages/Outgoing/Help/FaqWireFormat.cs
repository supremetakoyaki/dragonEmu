using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Help;

internal static class FaqWireFormat
{
    public static void AppendQuestions(ServerPacket packet, IReadOnlyList<FaqEntry> entries)
    {
        packet.AppendInt32(entries.Count);
        foreach (FaqEntry entry in entries)
        {
            packet.AppendInt32(entry.Id);
            packet.AppendString(entry.Question);
        }
    }
}
