namespace DragonEmu.Domain;

public enum RoomChatType
{
    Chat,
    Shout,
    Whisper,
}

public sealed record RoomChatRecord(DateTime SaidAt, int UserId, int RoomId, string Message, RoomChatType Type, int? WhisperedUserId, string IpAddress);

public sealed record InstantMessageRecord(DateTime SentAt, int FromUserId, int ToUserId, string Message, string IpAddress);

public sealed record CatalogPurchaseRecord(
    DateTime PurchasedAt, int UserId, int PageId, int OfferId, int Credits, int ActivityPoints, int? GiftReceiverId, string IpAddress);

public sealed record RoomVisitRecord(int UserId, int RoomId, DateTime EnteredAt, DateTime LeftAt);

public enum HistoryKind
{
    CatalogPurchases,
    RoomChat,
    InstantMessages,
    Trading,
    RoomVisits,
    ModerationActions,
    MarketplaceSales,
}

public sealed record HistoryRetentionDays(
    int CataloguePurchases, int RoomChat, int InstantMessages, int Trading, int RoomVisits = 30, int ModerationActions = 0,
    int MarketplaceSales = 90)
{
    public static readonly HistoryRetentionDays Defaults = new(30, 21, 30, 60, 30, 0, 90);

    public int For(HistoryKind kind) => kind switch
    {
        HistoryKind.CatalogPurchases => CataloguePurchases,
        HistoryKind.RoomChat => RoomChat,
        HistoryKind.InstantMessages => InstantMessages,
        HistoryKind.Trading => Trading,
        HistoryKind.RoomVisits => RoomVisits,
        HistoryKind.ModerationActions => ModerationActions,
        HistoryKind.MarketplaceSales => MarketplaceSales,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };
}
