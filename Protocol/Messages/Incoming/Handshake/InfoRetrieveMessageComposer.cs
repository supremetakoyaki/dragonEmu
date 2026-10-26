using DragonEmu.Game.Avatars;
using DragonEmu.Protocol.Messages.Outgoing.Handshake;

namespace DragonEmu.Protocol.Messages.Incoming.Handshake;

public sealed class InfoRetrieveMessageComposer : IMessageComposer
{
    private readonly RespectService _respect;

    public InfoRetrieveMessageComposer(RespectService respect)
    {
        _respect = respect;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account && !session.UserObjectSent)
        {
            session.UserObjectSent = true;
            (int received, int left, int petLeft) = await _respect.GetAsync(account.Id, cancellationToken);
            session.Send(new UserObjectEvent(account, received, left, petLeft).Compose());
        }
    }
}
