using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Help;

public sealed class FaqSearchResultsMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<FaqEntry> _entries;

    public FaqSearchResultsMessageEvent(IReadOnlyList<FaqEntry> entries)
    {
        _entries = entries;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Help.FaqSearchResultsMessageEvent);
        FaqWireFormat.AppendQuestions(packet, _entries);
        return packet;
    }
}
