namespace DragonEmu.Protocol.Messages.Outgoing.RoomPermissions;

public sealed class YouAreOwnerMessageEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.RoomPermissions.YouAreOwnerMessageEvent);
}
