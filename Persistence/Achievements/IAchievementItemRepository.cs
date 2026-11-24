namespace DragonEmu.Persistence;

public interface IAchievementItemRepository
{
    Task<bool> TryCountAsync(int userId, string codeBase, int itemId, CancellationToken cancellationToken = default);
}
