using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.RoomAction;

public sealed class BanUserMessageComposer : IMessageComposer
{
    private readonly RoomKickService _kicks;

    private int _userId;

    public BanUserMessageComposer(RoomKickService kicks)
    {
        _kicks = kicks;
    }

    public void Parse(ClientPacket packet)
    {
        _userId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _kicks.BanAsync(session, account, _userId, cancellationToken);
        }
    }
}
