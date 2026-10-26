namespace DragonEmu.Protocol.Messages.Outgoing.RoomSession;

public sealed class OpenConnectionMessageEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.RoomSession.OpenConnectionMessageEvent);
}
