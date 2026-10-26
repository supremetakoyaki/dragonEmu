using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Help;

public sealed class FaqCategoriesMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<FaqCategory> _categories;

    public FaqCategoriesMessageEvent(IReadOnlyList<FaqCategory> categories)
    {
        _categories = categories;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Help.FaqCategoriesMessageEvent);
        packet.AppendInt32(_categories.Count);
        foreach (FaqCategory category in _categories)
        {
            packet.AppendInt32(category.Id);
            packet.AppendString(category.Name);
            packet.AppendInt32(category.Entries.Count);
        }

        return packet;
    }
}
