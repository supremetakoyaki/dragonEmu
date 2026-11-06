using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IPhotoRepository
{
    Task<int> CreateAsync(
        int userId, int photoDefinitionId, int roomId, string caption, string token, byte[] png, DateTime takenAt,
        CancellationToken cancellationToken = default);

    Task<Photo?> GetByItemAsync(int itemId, CancellationToken cancellationToken = default);
}
