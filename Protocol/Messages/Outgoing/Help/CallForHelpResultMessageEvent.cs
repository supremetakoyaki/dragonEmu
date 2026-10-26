using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Help;

public sealed class CallForHelpResultMessageEvent : IMessageEvent
{
    public const int Sent = 0;
    public const int TooManyPending = 1;
    public const int HasAbusiveCall = 2;

    private readonly int _result;

    public CallForHelpResultMessageEvent(int result)
    {
        _result = result;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Help.CallForHelpResultMessageEvent);
        packet.AppendInt32(_result);
        return packet;
    }
}
