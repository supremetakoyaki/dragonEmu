using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IFireworkRepository
{
    Task<FireworkDefinition?> GetAsync(int furniDefId, CancellationToken cancellationToken = default);
}
