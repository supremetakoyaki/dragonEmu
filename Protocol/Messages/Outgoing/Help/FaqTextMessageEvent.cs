using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Help;

public sealed class FaqTextMessageEvent : IMessageEvent
{
    private readonly FaqEntry _entry;

    public FaqTextMessageEvent(FaqEntry entry)
    {
        _entry = entry;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Help.FaqTextMessageEvent);
        packet.AppendInt32(_entry.Id);
        packet.AppendString(_entry.Answer);
        return packet;
    }
}
