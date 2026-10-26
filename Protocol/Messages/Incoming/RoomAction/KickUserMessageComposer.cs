using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.RoomAction;

public sealed class KickUserMessageComposer : IMessageComposer
{
    private readonly RoomKickService _kicks;

    private int _userId;

    public KickUserMessageComposer(RoomKickService kicks)
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
            await _kicks.KickAsync(session, account, _userId, cancellationToken);
        }
    }
}
