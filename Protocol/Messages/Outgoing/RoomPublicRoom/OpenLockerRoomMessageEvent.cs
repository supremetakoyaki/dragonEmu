namespace DragonEmu.Protocol.Messages.Outgoing.RoomPublicRoom;

public sealed class OpenLockerRoomMessageEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.RoomPublicRoom.OpenLockerRoomMessageEvent);
}
