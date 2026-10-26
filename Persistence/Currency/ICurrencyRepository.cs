namespace DragonEmu.Persistence;

public interface ICurrencyRepository
{
    Task<long> GetBalanceAsync(int userId, int currency, CancellationToken cancellationToken = default);

    Task SetBalanceAsync(int userId, int currency, long value, CancellationToken cancellationToken = default);

    Task SeedDefaultBalancesAsync(int userId, CancellationToken cancellationToken = default);

    Task<long> AddBalanceAsync(int userId, int currency, long delta, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, long>> AddBalanceToManyAsync(
        IReadOnlyCollection<int> userIds, int currency, long delta, CancellationToken cancellationToken = default);
}
