namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public sealed class VoucherRedeemErrorMessageEvent : IMessageEvent
{
    public const string InvalidFormat = "0";
    public const string NotRedeemable = "1";

    private readonly string _errorCode;

    public VoucherRedeemErrorMessageEvent(string errorCode)
    {
        _errorCode = errorCode;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Catalog.VoucherRedeemErrorMessageEvent);
        packet.AppendString(_errorCode);
        return packet;
    }
}
