using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IRecyclerPrizeRepository
{
    Task<IReadOnlyList<RecyclerLevel>> GetLevelsAsync(CancellationToken cancellationToken = default);
}
