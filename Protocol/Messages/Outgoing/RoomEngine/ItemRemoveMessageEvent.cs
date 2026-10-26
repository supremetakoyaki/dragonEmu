namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class ItemRemoveMessageEvent : IMessageEvent
{
    private readonly int _itemId;

    public ItemRemoveMessageEvent(int itemId)
    {
        _itemId = itemId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.ItemRemoveMessageEvent);
        packet.AppendString(_itemId.ToString());
        return packet;
    }
}
