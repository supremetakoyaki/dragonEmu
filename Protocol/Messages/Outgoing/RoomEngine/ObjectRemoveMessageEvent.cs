namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class ObjectRemoveMessageEvent : IMessageEvent
{
    private readonly int _itemId;

    public ObjectRemoveMessageEvent(int itemId)
    {
        _itemId = itemId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.ObjectRemoveMessageEvent);
        packet.AppendString(_itemId.ToString());
        packet.AppendInt32(1);
        return packet;
    }
}
