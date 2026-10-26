using DragonEmu.Game.Ignoring;

namespace DragonEmu.Protocol.Messages.Incoming.Users;

public sealed class UnignoreUserMessageComposer : IMessageComposer
{
    private readonly IgnoreService _ignores;

    private string _username = string.Empty;

    public UnignoreUserMessageComposer(IgnoreService ignores)
    {
        _ignores = ignores;
    }

    public void Parse(ClientPacket packet)
    {
        _username = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _ignores.UnignoreAsync(session, account, _username, cancellationToken);
        }
    }
}
