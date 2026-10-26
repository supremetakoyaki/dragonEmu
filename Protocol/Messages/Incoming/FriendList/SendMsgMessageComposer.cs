using DragonEmu.Game.Messenger;

namespace DragonEmu.Protocol.Messages.Incoming.FriendList;

public sealed class SendMsgMessageComposer : IMessageComposer
{
    private readonly MessengerService _messenger;

    private int _friendId;
    private string _message = string.Empty;

    public SendMsgMessageComposer(MessengerService messenger)
    {
        _messenger = messenger;
    }

    public void Parse(ClientPacket packet)
    {
        _friendId = packet.PopInt32();
        _message = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _messenger.SendMessageAsync(session, account, _friendId, _message, cancellationToken);
        }
    }
}
