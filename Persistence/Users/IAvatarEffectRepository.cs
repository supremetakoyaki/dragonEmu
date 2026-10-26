using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IAvatarEffectRepository
{
    Task<IReadOnlyList<AvatarEffect>> GetAsync(int userId, DateTime utcNow, CancellationToken cancellationToken = default);

    Task AddAsync(int userId, int effectId, int duration, int quantity, CancellationToken cancellationToken = default);

    Task<AvatarEffect?> TryAddRunningAsync(int userId, int effectId, int duration, DateTime utcNow, CancellationToken cancellationToken = default);

    Task<AvatarEffect?> TryActivateAsync(int userId, int effectId, DateTime utcNow, CancellationToken cancellationToken = default);

    Task<bool> TryExpireAsync(int userId, int effectId, DateTime utcNow, CancellationToken cancellationToken = default);

    Task<bool> IsRunningAsync(int userId, int effectId, DateTime utcNow, CancellationToken cancellationToken = default);
}
