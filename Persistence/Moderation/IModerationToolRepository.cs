using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IModerationToolRepository
{
    Task RecordActionAsync(ModerationActionRecord action, CancellationToken cancellationToken = default);

    Task<ModeratorUserInfo?> GetUserInfoAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoomVisit>> GetVisitsAsync(int userId, int limit, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoomVisit>> GetVisitsWithChatAsync(int userId, int limit, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChatlogLine>> GetRoomChatAsync(int roomId, DateTime? from, DateTime? to, int limit, CancellationToken cancellationToken = default);
}
