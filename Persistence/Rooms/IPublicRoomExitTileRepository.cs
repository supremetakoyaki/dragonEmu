using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IPublicRoomExitTileRepository
{
    Task<IReadOnlyList<PublicRoomExitTile>> GetAllAsync(CancellationToken cancellationToken = default);
}
