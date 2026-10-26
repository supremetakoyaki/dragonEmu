namespace DragonEmu.Persistence;

public interface IIgnoreRepository
{
    Task<IReadOnlyList<(int UserId, string Username)>> GetIgnoredAsync(int userId, CancellationToken cancellationToken = default);

    Task<(bool Added, IReadOnlyList<int> Dropped)> AddAsync(int userId, int ignoredUserId, int max, CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(int userId, int ignoredUserId, CancellationToken cancellationToken = default);
}
