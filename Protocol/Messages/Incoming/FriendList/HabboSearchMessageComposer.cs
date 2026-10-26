using DragonEmu.Game.Messenger;

namespace DragonEmu.Protocol.Messages.Incoming.FriendList;

public sealed class HabboSearchMessageComposer : IMessageComposer
{
    private readonly MessengerService _messenger;

    private string _query = string.Empty;

    public HabboSearchMessageComposer(MessengerService messenger)
    {
        _messenger = messenger;
    }

    public void Parse(ClientPacket packet)
    {
        _query = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _messenger.SearchAsync(session, account, _query, cancellationToken);
        }
    }
}
