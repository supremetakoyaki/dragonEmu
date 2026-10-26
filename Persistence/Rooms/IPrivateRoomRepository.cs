using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IPrivateRoomRepository
{
    Task<IReadOnlyList<PrivateRoomSummary>> GetOwnedByAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrivateRoomSummary>> GetFavouritedByAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrivateRoomSummary>> GetVisitedByAsync(int userId, int limit, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrivateRoomSummary>> GetHighestScoredAsync(int limit, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrivateRoomSummary>> SearchByTextAsync(
        string text, IReadOnlyCollection<int> occupiedRoomIds, int limit, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrivateRoomSummary>> GetOwnedByFriendsOfAsync(
        int userId, IReadOnlyCollection<int> occupiedRoomIds, int limit, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrivateRoomSummary>> SearchByTagAsync(
        string tag, IReadOnlyCollection<int> occupiedRoomIds, int limit, CancellationToken cancellationToken = default);

    Task<PrivateRoomSummary?> GetByIdAsync(int roomId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrivateRoomSummary>> GetPublicAsync(CancellationToken cancellationToken = default);

    Task<int> CountOwnedByAsync(int userId, CancellationToken cancellationToken = default);

    Task<PrivateRoomSummary> CreateAsync(
        int ownerId, string label, string description, int categoryId, int heightmapId, CancellationToken cancellationToken = default);

    Task UpdatePaintAsync(int roomId, RoomPaintType type, string value, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>?> DeleteAsync(int roomId, CancellationToken cancellationToken = default);

    Task<int> LockRoomsWithBrokenPasswordsAsync(CancellationToken cancellationToken = default);

    Task SaveIconAsync(int roomId, int background, int foreground, IReadOnlyList<RoomIconItem> items, CancellationToken cancellationToken = default);

    Task SetQueuetileSpeedAsync(int roomId, int speed, CancellationToken cancellationToken = default);

    Task<PrivateRoomSummary> UpdateSettingsAsync(
        int roomId, string label, string description, int categoryId, int accessType, string? password, int maxUsers,
        IReadOnlyList<string> tags, bool allowPets, bool allowPetsEat, bool allowWalkthrough, CancellationToken cancellationToken = default);
}
