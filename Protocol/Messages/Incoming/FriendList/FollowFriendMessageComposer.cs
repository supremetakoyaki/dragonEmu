using DragonEmu.Game.Messenger;

namespace DragonEmu.Protocol.Messages.Incoming.FriendList;

public sealed class FollowFriendMessageComposer : IMessageComposer
{
    private readonly MessengerService _messenger;

    private int _friendId;

    public FollowFriendMessageComposer(MessengerService messenger)
    {
        _messenger = messenger;
    }

    public void Parse(ClientPacket packet)
    {
        _friendId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _messenger.FollowAsync(session, account, _friendId, cancellationToken);
        }
    }
}
