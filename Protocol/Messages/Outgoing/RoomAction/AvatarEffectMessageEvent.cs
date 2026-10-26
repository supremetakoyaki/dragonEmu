namespace DragonEmu.Protocol.Messages.Outgoing.RoomAction;

public sealed class AvatarEffectMessageEvent : IMessageEvent
{
    private readonly int _virtualId;
    private readonly int _effectId;

    public AvatarEffectMessageEvent(int virtualId, int effectId)
    {
        _virtualId = virtualId;
        _effectId = effectId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomAction.AvatarEffectMessageEvent);
        packet.AppendInt32(_virtualId);
        packet.AppendInt32(_effectId);
        return packet;
    }
}
