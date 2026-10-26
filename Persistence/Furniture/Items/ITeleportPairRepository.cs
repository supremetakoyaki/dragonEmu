using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface ITeleportPairRepository
{
    Task CreateAsync(int itemId1, int itemId2, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeleportPair>> GetByItemIdAsync(int itemId, CancellationToken cancellationToken = default);
}
