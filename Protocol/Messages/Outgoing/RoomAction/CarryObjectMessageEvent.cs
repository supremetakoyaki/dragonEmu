namespace DragonEmu.Protocol.Messages.Outgoing.RoomAction;

public sealed class CarryObjectMessageEvent : IMessageEvent
{
    private readonly int _virtualId;
    private readonly int _itemId;

    public CarryObjectMessageEvent(int virtualId, int itemId)
    {
        _virtualId = virtualId;
        _itemId = itemId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomAction.CarryObjectMessageEvent);
        packet.AppendInt32(_virtualId);
        packet.AppendInt32(_itemId);
        if (_itemId > 0)
        {
            packet.AppendString(string.Empty);
        }

        return packet;
    }
}
