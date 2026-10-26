namespace DragonEmu.Persistence;

public interface IRoomVisitRepository
{
    Task<bool> RecordVisitAsync(int userId, int roomId, CancellationToken cancellationToken = default);
}
