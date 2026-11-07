using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IPublicRoomFurnitureRepository
{
    Task<IReadOnlyList<PublicRoomFurnitureItem>> GetForRoomAsync(int roomId, string heightmapLabel, CancellationToken cancellationToken = default);
}
