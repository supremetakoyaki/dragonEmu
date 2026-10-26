using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IRoomAdRepository
{
    Task<IReadOnlyList<RoomAd>> GetForRoomAsync(int roomId, CancellationToken cancellationToken = default);
}
