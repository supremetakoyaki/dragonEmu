using DragonEmu.Game.Avatars;

namespace DragonEmu.Protocol.Messages.Incoming.Users;

public sealed class RespectUserMessageComposer : IMessageComposer
{
    private readonly RespectService _respect;

    private int _userId;

    public RespectUserMessageComposer(RespectService respect)
    {
        _respect = respect;
    }

    public void Parse(ClientPacket packet)
    {
        _userId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _respect.GiveAsync(session, account, _userId, cancellationToken);
        }
    }
}
