using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IPublicRoomWarpTileRepository
{
    Task<IReadOnlyList<PublicRoomWarpTile>> GetAllAsync(CancellationToken cancellationToken = default);
}
