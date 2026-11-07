namespace DragonEmu.Protocol.Messages.Outgoing.RoomSession;

public sealed class YouAreSpectatorMessageEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.RoomSession.YouAreSpectatorMessageEvent);
}
