using System.Runtime.CompilerServices;

namespace DragonEmu.Game.Rooms;

public sealed class PerRoomLock
{
    private readonly ConditionalWeakTable<Room, SemaphoreSlim> _locks = new();

    public async Task RunAsync(Room room, Func<Task> work, CancellationToken cancellationToken)
    {
        SemaphoreSlim roomLock = _locks.GetValue(room, _ => new SemaphoreSlim(1, 1));
        await roomLock.WaitAsync(cancellationToken);
        try
        {
            await work();
        }
        finally
        {
            roomLock.Release();
        }
    }
}
