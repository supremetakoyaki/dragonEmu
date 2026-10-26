using DragonEmu.Game.Avatars;

namespace DragonEmu.Protocol.Messages.Incoming.Avatar;

public sealed class GetWardrobeMessageComposer : IMessageComposer
{
    private readonly WardrobeService _wardrobe;

    public GetWardrobeMessageComposer(WardrobeService wardrobe)
    {
        _wardrobe = wardrobe;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _wardrobe.SendAsync(session, account, cancellationToken);
        }
    }
}
