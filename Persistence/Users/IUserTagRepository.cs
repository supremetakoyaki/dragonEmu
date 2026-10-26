namespace DragonEmu.Persistence;

public interface IUserTagRepository
{
    Task<IReadOnlyList<string>> GetAsync(int userId, int limit, CancellationToken cancellationToken = default);
}
