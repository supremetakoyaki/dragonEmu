using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IRespectRepository
{
    Task<RespectStanding> GetAsync(int userId, CancellationToken cancellationToken = default);

    Task<RespectGiven?> TryGiveAsync(int giverId, int targetId, DateOnly today, int dailyGrant, CancellationToken cancellationToken = default);
}
