namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class SoundMachineSelectedMessageEvent : IMessageEvent
{
    private readonly int _itemId;
    private readonly bool _on;

    public SoundMachineSelectedMessageEvent(int itemId, bool on)
    {
        _itemId = itemId;
        _on = on;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Trax.SoundMachineSelectedMessageEvent);
        packet.AppendInt32(_itemId);
        packet.AppendBoolean(_on);
        return packet;
    }
}
