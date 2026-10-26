using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface INavigatorPublicRoomCategoryRepository
{
    Task<IReadOnlyList<NavigatorPublicRoomCategory>> GetAllAsync(CancellationToken cancellationToken = default);
}
