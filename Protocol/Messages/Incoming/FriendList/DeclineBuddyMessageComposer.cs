using DragonEmu.Game.Messenger;

namespace DragonEmu.Protocol.Messages.Incoming.FriendList;

public sealed class DeclineBuddyMessageComposer : IMessageComposer
{
    private const int MaxIds = 1000;

    private readonly MessengerService _messenger;

    private bool _all;
    private IReadOnlyList<int> _requesterIds = [];

    public DeclineBuddyMessageComposer(MessengerService messenger)
    {
        _messenger = messenger;
    }

    public void Parse(ClientPacket packet)
    {
        _all = packet.PopInt32() == 1;
        _requesterIds = packet.PopInt32List(MaxIds);
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _messenger.DeclineAsync(account, _all, _requesterIds, cancellationToken);
        }
    }
}
