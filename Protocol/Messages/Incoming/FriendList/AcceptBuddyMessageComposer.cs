using DragonEmu.Game.Messenger;

namespace DragonEmu.Protocol.Messages.Incoming.FriendList;

public sealed class AcceptBuddyMessageComposer : IMessageComposer
{
    private const int MaxIds = 1000;

    private readonly MessengerService _messenger;

    private IReadOnlyList<int> _requesterIds = [];

    public AcceptBuddyMessageComposer(MessengerService messenger)
    {
        _messenger = messenger;
    }

    public void Parse(ClientPacket packet)
    {
        _requesterIds = packet.PopInt32List(MaxIds);
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _messenger.AcceptAsync(session, account, _requesterIds, cancellationToken);
        }
    }
}
