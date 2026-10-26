using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IPublicRoomLibraryRepository
{
    Task<IReadOnlyList<PublicRoomLibraries>> GetAllAsync(CancellationToken cancellationToken = default);
}
