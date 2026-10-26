namespace DragonEmu.Protocol.Messages.Outgoing.RoomSession;

public sealed class FlatAccessibleMessageEvent : IMessageEvent
{
    private readonly string _userName;

    public FlatAccessibleMessageEvent(string userName = "")
    {
        _userName = userName;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomSession.FlatAccessibleMessageEvent);
        packet.AppendString(_userName);
        return packet;
    }
}
