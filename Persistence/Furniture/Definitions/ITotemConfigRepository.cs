using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface ITotemConfigRepository
{
    Task<IReadOnlyList<TotemCombination>> GetAllAsync(CancellationToken cancellationToken = default);
}
