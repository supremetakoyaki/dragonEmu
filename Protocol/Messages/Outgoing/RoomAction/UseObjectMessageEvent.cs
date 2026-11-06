namespace DragonEmu.Protocol.Messages.Outgoing.RoomAction;

public sealed class UseObjectMessageEvent : IMessageEvent
{
    private readonly int _virtualId;
    private readonly int _itemId;

    public UseObjectMessageEvent(int virtualId, int itemId)
    {
        _virtualId = virtualId;
        _itemId = itemId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomAction.UseObjectMessageEvent);
        packet.AppendInt32(_virtualId);
        packet.AppendInt32(_itemId);
        return packet;
    }
}
