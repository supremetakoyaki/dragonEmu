using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.InventoryAvatarEffect;

public sealed class AvatarEffectsMessageEvent : IMessageEvent
{
    private const int NotRunning = -1;

    private readonly IReadOnlyList<AvatarEffect> _effects;
    private readonly DateTime _utcNow;

    public AvatarEffectsMessageEvent(IReadOnlyList<AvatarEffect> effects, DateTime utcNow)
    {
        _effects = effects;
        _utcNow = utcNow;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryAvatarEffect.AvatarEffectsMessageEvent);
        packet.AppendInt32(_effects.Count);

        foreach (AvatarEffect effect in _effects)
        {
            packet.AppendInt32(effect.EffectId);
            packet.AppendInt32(effect.Duration);
            packet.AppendInt32(effect.Quantity);
            packet.AppendInt32(effect.SecondsLeft(_utcNow) ?? NotRunning);
        }

        return packet;
    }
}
