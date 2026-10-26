using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Help;

public sealed class CallForHelpReplyMessageEvent : IMessageEvent
{
    private readonly string _message;

    public CallForHelpReplyMessageEvent(string message)
    {
        _message = message;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Help.CallForHelpReplyMessageEvent);
        packet.AppendString(_message);
        return packet;
    }
}
