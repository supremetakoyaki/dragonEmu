namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class ItemDataUpdateMessageEvent : IMessageEvent
{
    private readonly int _itemId;
    private readonly string _data;

    public ItemDataUpdateMessageEvent(int itemId, string data)
    {
        _itemId = itemId;
        _data = data;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.ItemDataUpdateMessageEvent);
        packet.AppendString(_itemId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        packet.AppendString(_data);
        return packet;
    }
}
