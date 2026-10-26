using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IFurnitureAliasRepository
{
    Task<IReadOnlyList<FurnitureAlias>> GetAllEnabledAsync(CancellationToken cancellationToken = default);
}
