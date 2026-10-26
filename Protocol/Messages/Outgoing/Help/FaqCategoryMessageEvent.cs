using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Help;

public sealed class FaqCategoryMessageEvent : IMessageEvent
{
    private readonly FaqCategory _category;

    public FaqCategoryMessageEvent(FaqCategory category)
    {
        _category = category;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Help.FaqCategoryMessageEvent);
        packet.AppendInt32(_category.Id);
        packet.AppendString(_category.Description);
        FaqWireFormat.AppendQuestions(packet, _category.Entries);
        return packet;
    }
}
