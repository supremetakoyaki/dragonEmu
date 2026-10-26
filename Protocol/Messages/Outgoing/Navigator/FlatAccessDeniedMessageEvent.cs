namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class FlatAccessDeniedMessageEvent : IMessageEvent
{
    private readonly string _userName;

    public FlatAccessDeniedMessageEvent(string userName = "")
    {
        _userName = userName;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.FlatAccessDeniedMessageEvent);
        packet.AppendString(_userName);
        return packet;
    }
}
