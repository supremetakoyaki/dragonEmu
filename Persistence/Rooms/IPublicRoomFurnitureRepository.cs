using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IPublicRoomFurnitureRepository
{
    Task<IReadOnlyList<PublicRoomFurnitureItem>> GetForModelAsync(string heightmapLabel, CancellationToken cancellationToken = default);
}
