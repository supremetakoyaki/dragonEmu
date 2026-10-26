namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class UserRemoveMessageEvent : IMessageEvent
{
    private readonly int _virtualId;

    public UserRemoveMessageEvent(int virtualId)
    {
        _virtualId = virtualId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.UserRemoveMessageEvent);
        packet.AppendString(_virtualId.ToString());
        return packet;
    }
}
