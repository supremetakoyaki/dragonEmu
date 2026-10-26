namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class DoorbellMessageEvent : IMessageEvent
{
    private readonly string _userName;

    public DoorbellMessageEvent(string userName = "")
    {
        _userName = userName;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.DoorbellMessageEvent);
        packet.AppendString(_userName);
        return packet;
    }
}
