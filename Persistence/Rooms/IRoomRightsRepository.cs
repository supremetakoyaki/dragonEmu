using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IRoomRightsRepository
{
    Task<bool> HasRightsAsync(int userId, int roomId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoomRightsHolder>> GetHoldersAsync(int roomId, int limit, CancellationToken cancellationToken = default);

    Task<int> GetHolderCountAsync(int roomId, CancellationToken cancellationToken = default);

    Task<bool> GrantAsync(int userId, int roomId, CancellationToken cancellationToken = default);

    Task<bool> RevokeAsync(int userId, int roomId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> RevokeAllAsync(int roomId, CancellationToken cancellationToken = default);

    Task BanAsync(int userId, int roomId, int bannedById, int seconds, CancellationToken cancellationToken = default);

    Task<bool> IsBannedAsync(int userId, int roomId, CancellationToken cancellationToken = default);
}
