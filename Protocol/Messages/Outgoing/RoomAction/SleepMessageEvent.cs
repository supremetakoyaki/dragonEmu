namespace DragonEmu.Protocol.Messages.Outgoing.RoomAction;

public sealed class SleepMessageEvent : IMessageEvent
{
    private readonly int _virtualId;
    private readonly bool _asleep;

    public SleepMessageEvent(int virtualId, bool asleep)
    {
        _virtualId = virtualId;
        _asleep = asleep;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomAction.SleepMessageEvent);
        packet.AppendInt32(_virtualId);
        packet.AppendBoolean(_asleep);
        return packet;
    }
}
