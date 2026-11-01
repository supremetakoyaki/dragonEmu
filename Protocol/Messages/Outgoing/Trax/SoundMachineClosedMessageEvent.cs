namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class SoundMachineClosedMessageEvent : IMessageEvent
{
    private readonly int _itemId;

    public SoundMachineClosedMessageEvent(int itemId)
    {
        _itemId = itemId;
    }

    public static SoundMachineClosedMessageEvent All { get; } = new(0);

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Trax.SoundMachineClosedMessageEvent);
        packet.AppendInt32(_itemId);
        return packet;
    }
}
