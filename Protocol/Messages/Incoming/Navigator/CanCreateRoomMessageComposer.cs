using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class CanCreateRoomMessageComposer : IMessageComposer
{
    private readonly RoomCreationLimit _limit;

    public CanCreateRoomMessageComposer(RoomCreationLimit limit)
    {
        _limit = limit;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        bool canCreate = await _limit.MayCreateAsync(account, cancellationToken);
        session.Send(new CanCreateRoomEvent(canCreate ? 0 : 1, _limit.Limit).Compose());
    }
}
