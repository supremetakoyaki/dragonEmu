namespace DragonEmu.Protocol.Messages.Outgoing.RoomAction;

public sealed class AvatarEffectMessageEvent : IMessageEvent
{
    private readonly int _virtualId;
    private readonly int _effectId;
    private readonly int _delayMilliseconds;

    public AvatarEffectMessageEvent(int virtualId, int effectId, int delayMilliseconds = 0)
    {
        _delayMilliseconds = delayMilliseconds;
        _virtualId = virtualId;
        _effectId = effectId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomAction.AvatarEffectMessageEvent);
        packet.AppendInt32(_virtualId);
        packet.AppendInt32(_effectId);
        packet.AppendInt32(_delayMilliseconds);
        return packet;
    }
}
