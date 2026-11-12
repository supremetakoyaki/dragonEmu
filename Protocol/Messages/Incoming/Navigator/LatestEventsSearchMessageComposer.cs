using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class LatestEventsSearchMessageComposer : IMessageComposer
{
    private readonly RoomEventService _events;

    private string _category = string.Empty;

    public LatestEventsSearchMessageComposer(RoomEventService events)
    {
        _events = events;
    }

    public void Parse(ClientPacket packet)
    {
        _category = packet.PopString();
        AdIndex = packet.RemainingLength > 0 ? packet.PopInt32() : 0;
    }

    public int AdIndex { get; private set; }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not null)
        {
            _events.SendEvents(session, _category);
        }

        return ValueTask.CompletedTask;
    }
}
