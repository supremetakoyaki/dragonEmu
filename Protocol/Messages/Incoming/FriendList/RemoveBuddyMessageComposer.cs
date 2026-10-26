using DragonEmu.Game.Messenger;

namespace DragonEmu.Protocol.Messages.Incoming.FriendList;

public sealed class RemoveBuddyMessageComposer : IMessageComposer
{
    private const int MaxIds = 1000;

    private readonly MessengerService _messenger;

    private IReadOnlyList<int> _friendIds = [];

    public RemoveBuddyMessageComposer(MessengerService messenger)
    {
        _messenger = messenger;
    }

    public void Parse(ClientPacket packet)
    {
        _friendIds = packet.PopInt32List(MaxIds);
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _messenger.RemoveAsync(session, account, _friendIds, cancellationToken);
        }
    }
}
