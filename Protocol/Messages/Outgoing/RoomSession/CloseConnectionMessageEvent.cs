namespace DragonEmu.Protocol.Messages.Outgoing.RoomSession;

public sealed class CloseConnectionMessageEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.RoomSession.CloseConnectionMessageEvent);
}
