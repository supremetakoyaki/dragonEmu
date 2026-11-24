namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public sealed class ChargeInfoMessageEvent : IMessageEvent
{
    private readonly int _stuffId;
    private readonly int _charges;
    private readonly int _priceInCredits;
    private readonly int _priceInActivityPoints;
    private readonly int _activityPointType;
    private readonly int _chargePatchSize;

    public ChargeInfoMessageEvent(int stuffId, int charges, int priceInCredits, int priceInActivityPoints, int activityPointType, int chargePatchSize)
    {
        _stuffId = stuffId;
        _charges = charges;
        _priceInCredits = priceInCredits;
        _priceInActivityPoints = priceInActivityPoints;
        _activityPointType = activityPointType;
        _chargePatchSize = chargePatchSize;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Catalog.ChargeInfoMessageEvent);
        packet.AppendInt32(_stuffId);
        packet.AppendInt32(_charges);
        packet.AppendInt32(_priceInCredits);
        packet.AppendInt32(_priceInActivityPoints);
        packet.AppendInt32(_activityPointType);
        packet.AppendInt32(_chargePatchSize);
        return packet;
    }
}
