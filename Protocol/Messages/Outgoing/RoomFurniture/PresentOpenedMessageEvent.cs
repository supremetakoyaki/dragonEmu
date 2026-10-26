namespace DragonEmu.Protocol.Messages.Outgoing.RoomFurniture;

public sealed class PresentOpenedMessageEvent : IMessageEvent
{
    private const string FloorTypeCode = "s";
    private const string WallTypeCode = "i";
    private const string SubscriptionTypeCode = "h";

    private readonly string _typeCode;
    private readonly int _refFurnidata;

    public PresentOpenedMessageEvent(int refFurnidata, bool isWallItem)
    {
        _typeCode = isWallItem ? WallTypeCode : FloorTypeCode;
        _refFurnidata = refFurnidata;
    }

    private PresentOpenedMessageEvent(string typeCode, int refFurnidata)
    {
        _typeCode = typeCode;
        _refFurnidata = refFurnidata;
    }

    public static PresentOpenedMessageEvent Subscription() => new(SubscriptionTypeCode, refFurnidata: 0);

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomFurniture.PresentOpenedMessageEvent);
        packet.AppendString(_typeCode);
        packet.AppendInt32(_refFurnidata);
        return packet;
    }
}
