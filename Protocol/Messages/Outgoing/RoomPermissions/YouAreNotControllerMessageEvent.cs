namespace DragonEmu.Protocol.Messages.Outgoing.RoomPermissions;

public sealed class YouAreNotControllerMessageEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.RoomPermissions.YouAreNotControllerMessageEvent);
}
