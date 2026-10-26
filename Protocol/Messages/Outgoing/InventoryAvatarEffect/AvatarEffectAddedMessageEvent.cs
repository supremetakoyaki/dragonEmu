namespace DragonEmu.Protocol.Messages.Outgoing.InventoryAvatarEffect;

public sealed class AvatarEffectAddedMessageEvent : IMessageEvent
{
    private readonly int _effectId;
    private readonly int _duration;

    public AvatarEffectAddedMessageEvent(int effectId, int duration)
    {
        _effectId = effectId;
        _duration = duration;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryAvatarEffect.AvatarEffectAddedMessageEvent);
        packet.AppendInt32(_effectId);
        packet.AppendInt32(_duration);
        return packet;
    }
}
