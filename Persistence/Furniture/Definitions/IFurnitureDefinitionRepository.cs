using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IFurnitureDefinitionRepository
{
    Task<IReadOnlyList<FurnitureDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
}
