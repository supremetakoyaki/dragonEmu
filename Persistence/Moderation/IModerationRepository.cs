namespace DragonEmu.Persistence;

public sealed record UserBan(int Id, string Reason, DateTime ExpiresAt, bool Superban);

public interface IModerationRepository
{
    Task BanAsync(int userId, int bannedById, bool superban, string reason, DateTime expiresAt, CancellationToken cancellationToken = default);

    Task<UserBan?> GetActiveBanAsync(int userId, CancellationToken cancellationToken = default);

    Task MuteAsync(int userId, int mutedById, DateTime expiresAt, CancellationToken cancellationToken = default);

    Task<DateTime?> GetMuteAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> UnmuteAsync(int userId, CancellationToken cancellationToken = default);

    Task MuteRoomAsync(int roomId, int mutedById, DateTime expiresAt, CancellationToken cancellationToken = default);

    Task<DateTime?> GetRoomMuteAsync(int roomId, CancellationToken cancellationToken = default);

    Task<bool> UnmuteRoomAsync(int roomId, CancellationToken cancellationToken = default);
}
