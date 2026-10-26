using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IHistoryRepository
{
    Task AddRoomChatAsync(IReadOnlyList<RoomChatRecord> records, CancellationToken cancellationToken = default);

    Task AddInstantMessagesAsync(IReadOnlyList<InstantMessageRecord> records, CancellationToken cancellationToken = default);

    Task AddCatalogPurchasesAsync(IReadOnlyList<CatalogPurchaseRecord> records, CancellationToken cancellationToken = default);

    Task AddRoomVisitsAsync(IReadOnlyList<RoomVisitRecord> records, CancellationToken cancellationToken = default);

    Task<HistoryRetentionDays> GetRetentionAsync(CancellationToken cancellationToken = default);

    Task<int> DeleteOlderThanAsync(HistoryKind kind, int days, int limit, CancellationToken cancellationToken = default);
}
