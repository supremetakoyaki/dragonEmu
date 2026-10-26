namespace DragonEmu.Protocol.Messages.Outgoing.Handshake;

public sealed class AuthenticationOKMessageEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.Handshake.AuthenticationOKMessageEvent);
}
