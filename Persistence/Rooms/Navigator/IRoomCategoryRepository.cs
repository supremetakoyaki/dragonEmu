using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IRoomCategoryRepository
{
    Task<IReadOnlyList<RoomCategory>> GetAllAsync(CancellationToken cancellationToken = default);
}
