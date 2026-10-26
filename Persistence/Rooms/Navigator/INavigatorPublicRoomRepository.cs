using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface INavigatorPublicRoomRepository
{
    Task<IReadOnlyList<NavigatorPublicRoom>> GetAllAsync(CancellationToken cancellationToken = default);
}
