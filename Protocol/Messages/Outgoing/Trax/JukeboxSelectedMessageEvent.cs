namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class JukeboxSelectedMessageEvent : IMessageEvent
{
    private readonly int _itemId;
    private readonly bool _mayManage;

    public JukeboxSelectedMessageEvent(int itemId, bool mayManage)
    {
        _itemId = itemId;
        _mayManage = mayManage;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Trax.JukeboxSelectedMessageEvent);
        packet.AppendInt32(_itemId);
        packet.AppendBoolean(_mayManage);
        return packet;
    }
}
