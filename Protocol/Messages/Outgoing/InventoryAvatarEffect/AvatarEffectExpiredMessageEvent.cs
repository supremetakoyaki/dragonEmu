namespace DragonEmu.Protocol.Messages.Outgoing.InventoryAvatarEffect;

public sealed class AvatarEffectExpiredMessageEvent : IMessageEvent
{
    private readonly int _effectId;

    public AvatarEffectExpiredMessageEvent(int effectId)
    {
        _effectId = effectId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryAvatarEffect.AvatarEffectExpiredMessageEvent);
        packet.AppendInt32(_effectId);
        return packet;
    }
}
