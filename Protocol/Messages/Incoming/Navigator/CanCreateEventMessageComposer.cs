using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class CanCreateEventMessageComposer : IMessageComposer
{
    private readonly RoomEventService _events;

    public CanCreateEventMessageComposer(RoomEventService events)
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
            await _events.SendCanCreateAsync(session, account, cancellationToken);
        }
    }
}
