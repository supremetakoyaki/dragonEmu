using DragonEmu.Game.Ignoring;

namespace DragonEmu.Protocol.Messages.Incoming.Users;

public sealed class IgnoreUserMessageComposer : IMessageComposer
{
    private readonly IgnoreService _ignores;

    private string _username = string.Empty;

    public IgnoreUserMessageComposer(IgnoreService ignores)
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
            await _ignores.IgnoreAsync(session, account, _username, cancellationToken);
        }
    }
}
