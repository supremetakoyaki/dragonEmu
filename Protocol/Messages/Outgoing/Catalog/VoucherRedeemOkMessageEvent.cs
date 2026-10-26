namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public sealed class VoucherRedeemOkMessageEvent : IMessageEvent
{
    private readonly string _productDescription;
    private readonly string _productName;

    public VoucherRedeemOkMessageEvent(string productDescription = "", string productName = "")
    {
        _productDescription = productDescription;
        _productName = productName;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Catalog.VoucherRedeemOkMessageEvent);
        packet.AppendString(_productDescription);
        packet.AppendString(_productName);
        return packet;
    }
}
