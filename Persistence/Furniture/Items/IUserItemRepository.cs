using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IUserItemRepository
{
    Task<int> CreateInInventoryAsync(int userId, int furniDefId, string? extraData, CancellationToken cancellationToken = default);

    Task<int> CreateInInventoryAsync(int userId, int furniDefId, string? extraData, bool seen, CancellationToken cancellationToken = default);

    Task<int> CreateInInventoryAsync(int userId, int furniDefId, string? extraData, bool seen, int? extras, CancellationToken cancellationToken = default);

    Task<UserItem?> GetByIdAsync(int itemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserItem>> GetByIdsAsync(IReadOnlyList<int> itemIds, CancellationToken cancellationToken = default);

    Task SetRentalExpiryAsync(IReadOnlyCollection<int> itemIds, DateTime expiresAt, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExpiredRental>> GetExpiredRentalsAsync(int limit, CancellationToken cancellationToken = default);

    Task<bool> DeleteExpiredRentalAsync(int itemId, CancellationToken cancellationToken = default);

    Task<bool> UpdateExtraDataAsync(int itemId, string? extraData, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, string?>> GetExtraDataInRoomAsync(int roomId, IReadOnlyCollection<int> itemIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserItem>> GetInventoryAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserItem>> GetFloorItemsInRoomAsync(int roomId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserItem>> GetWallItemsInRoomAsync(int roomId, CancellationToken cancellationToken = default);

    Task<bool> PlaceOnFloorAsync(
        int itemId, int userId, int roomId, int x, int y, decimal z, int rotation, string? basalExtraData, bool resetExtraData,
        CancellationToken cancellationToken = default);

    Task<bool> PlaceOnWallAsync(
        int itemId, int userId, int roomId, string wallPosition, string? basalExtraData, bool resetExtraData,
        CancellationToken cancellationToken = default);

    Task<bool> MoveOnWallAsync(int itemId, int roomId, string wallPosition, CancellationToken cancellationToken = default);

    Task<bool> MoveOnFloorAsync(int itemId, int roomId, int x, int y, decimal z, int rotation, CancellationToken cancellationToken = default);

    Task SaveFloorPositionsAsync(int roomId, IReadOnlyList<FloorItemPosition> positions, CancellationToken cancellationToken = default);

    Task<StickiePlacement?> PlaceStickieAsync(
        int padId, int userId, int roomId, string wallPosition, string noteData, CancellationToken cancellationToken = default);

    Task<bool> DeleteFromRoomAsync(int itemId, int roomId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> PickAllAsync(int roomId, int newOwnerId, CancellationToken cancellationToken = default);

    Task<bool> DeleteFromInventoryAsync(int itemId, int userId, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int itemId, int userId, CancellationToken cancellationToken = default);

    Task<bool> PickupFromFloorAsync(int itemId, int roomId, int pickerUserId, CancellationToken cancellationToken = default);

    Task<bool> PickupFromWallAsync(int itemId, int roomId, int pickerUserId, CancellationToken cancellationToken = default);
}

public sealed record StickiePlacement(int NoteId, int NotesLeft);
