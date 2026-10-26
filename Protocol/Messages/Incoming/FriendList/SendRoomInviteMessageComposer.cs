using DragonEmu.Game.Messenger;
using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Incoming.FriendList;

public sealed class SendRoomInviteMessageComposer : IMessageComposer
{
    private const int MaxIds = 1000;

    private readonly MessengerService _messenger;

    private IReadOnlyList<int> _friendIds = [];
    private string _message = string.Empty;

    public SendRoomInviteMessageComposer(MessengerService messenger)
    {
        _messenger = messenger;
    }

    public void Parse(ClientPacket packet)
    {
        _friendIds = packet.PopInt32List(MaxIds);
        _message = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _messenger.InviteAsync(session, account, _friendIds, _message, cancellationToken);
        }
    }
}
