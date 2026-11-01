namespace DragonEmu.Persistence;

public sealed record WebCommand(long Id, int UserId, string Command);

public interface IWebCommandRepository
{
    Task<IReadOnlyList<WebCommand>> GetPendingAsync(int limit, CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
