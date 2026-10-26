using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IGiftWrapConfigRepository
{
    Task<GiftWrapConfig?> GetAsync(CancellationToken cancellationToken = default);
}
