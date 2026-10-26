using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Help;

public sealed class FaqClientFaqsMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<FaqEntry> _urgent;
    private readonly IReadOnlyList<FaqEntry> _normal;

    public FaqClientFaqsMessageEvent(IReadOnlyList<FaqEntry> urgent, IReadOnlyList<FaqEntry> normal)
    {
        _urgent = urgent;
        _normal = normal;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Help.FaqClientFaqsMessageEvent);
        FaqWireFormat.AppendQuestions(packet, _urgent);
        FaqWireFormat.AppendQuestions(packet, _normal);
        return packet;
    }
}
