using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface ITradeRepository
{
    Task<int?> CompleteAsync(CompletedTrade trade, CancellationToken cancellationToken = default);
}
