using DragonEmu.Game.Messenger;

namespace DragonEmu.Protocol.Messages.Incoming.FriendList;

public sealed class GetBuddyRequestsMessageComposer : IMessageComposer
{
    private readonly MessengerService _messenger;

    public GetBuddyRequestsMessageComposer(MessengerService messenger)
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
            await _messenger.SendRequestsAsync(session, account, cancellationToken);
        }
    }
}
