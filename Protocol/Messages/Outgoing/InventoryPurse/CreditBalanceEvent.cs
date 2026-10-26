namespace DragonEmu.Protocol.Messages.Outgoing.InventoryPurse;

public sealed class CreditBalanceEvent : IMessageEvent
{
    private readonly long _credits;

    public CreditBalanceEvent(long credits)
    {
        _credits = credits;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryPurse.CreditBalanceEvent);
        packet.AppendString($"{_credits}.0");
        return packet;
    }
}
