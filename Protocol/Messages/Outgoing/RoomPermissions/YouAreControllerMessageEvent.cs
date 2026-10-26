namespace DragonEmu.Protocol.Messages.Outgoing.RoomPermissions;

public sealed class YouAreControllerMessageEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.RoomPermissions.YouAreControllerMessageEvent);
}
