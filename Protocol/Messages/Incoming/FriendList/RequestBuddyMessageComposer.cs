using DragonEmu.Game.Messenger;

namespace DragonEmu.Protocol.Messages.Incoming.FriendList;

public sealed class RequestBuddyMessageComposer : IMessageComposer
{
    private readonly MessengerService _messenger;

    private string _username = string.Empty;

    public RequestBuddyMessageComposer(MessengerService messenger)
    {
        _messenger = messenger;
    }

    public void Parse(ClientPacket packet)
    {
        _username = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _messenger.RequestAsync(session, account, _username, cancellationToken);
        }
    }
}
