namespace DragonEmu.Protocol.Messages.Outgoing.Handshake;

public sealed class GenericErrorEvent : IMessageEvent
{
    private readonly int _errorCode;

    public GenericErrorEvent(int errorCode)
    {
        _errorCode = errorCode;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Handshake.GenericErrorEvent);
        packet.AppendInt32(_errorCode);
        return packet;
    }
}
