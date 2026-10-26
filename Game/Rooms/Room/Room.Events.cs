using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    private RoomEvent? _event;

    public RoomEvent? Event => Volatile.Read(ref _event);

    public event Action<Room>? EventEnded;

    public bool TryStartEvent(RoomEvent roomEvent) => Interlocked.CompareExchange(ref _event, roomEvent, null) is null;

    public bool TryReplaceEvent(RoomEvent current, RoomEvent updated) =>
        ReferenceEquals(Interlocked.CompareExchange(ref _event, updated, current), current);

    public bool EndEvent()
    {
        if (Interlocked.Exchange(ref _event, null) is null)
        {
            return false;
        }

        EventEnded?.Invoke(this);
        return true;
    }

    private void EndEventHostedBy(long sessionId)
    {
        if (Event is { } current && current.HostSessionId == sessionId
            && ReferenceEquals(Interlocked.CompareExchange(ref _event, null, current), current))
        {
            EventEnded?.Invoke(this);
        }
    }
}
