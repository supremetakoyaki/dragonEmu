using DragonEmu.Game.Messenger;

namespace DragonEmu.Protocol.Messages.Incoming.FriendList;

public sealed class MessengerInitMessageComposer : IMessageComposer
{
    private readonly MessengerService _messenger;

    public MessengerInitMessageComposer(MessengerService messenger)
    {
        _messenger = messenger;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _messenger.InitAsync(session, account, cancellationToken);
        }
    }
}
