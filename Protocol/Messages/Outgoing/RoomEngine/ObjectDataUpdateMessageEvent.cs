namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class ObjectDataUpdateMessageEvent : IMessageEvent
{
    private readonly int _itemId;
    private readonly string _data;

    public ObjectDataUpdateMessageEvent(int itemId, string data)
    {
        _itemId = itemId;
        _data = data;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.ObjectDataUpdateMessageEvent);
        packet.AppendString(_itemId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        packet.AppendString(_data);
        return packet;
    }
}
