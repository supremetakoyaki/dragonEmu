using DragonEmu.Game.Avatars;

namespace DragonEmu.Protocol.Messages.Incoming.InventoryAvatarEffect;

public sealed class AvatarEffectActivatedComposer : IMessageComposer
{
    private readonly AvatarEffectService _avatarEffectService;

    private int _effectId;

    public AvatarEffectActivatedComposer(AvatarEffectService avatarEffectService)
    {
        _avatarEffectService = avatarEffectService;
    }

    public void Parse(ClientPacket packet)
    {
        _effectId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _avatarEffectService.ActivateAsync(session, account.Id, _effectId, cancellationToken);
        }
    }
}
