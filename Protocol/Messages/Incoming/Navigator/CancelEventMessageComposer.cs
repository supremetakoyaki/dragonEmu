using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class CancelEventMessageComposer : IMessageComposer
{
    private readonly RoomEventService _events;

    public CancelEventMessageComposer(RoomEventService events)
    {
        _events = events;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _events.EndAsync(session, account, cancellationToken);
        }
    }
}
