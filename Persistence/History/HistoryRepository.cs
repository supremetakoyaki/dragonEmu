using System.Text;
using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class HistoryRepository : IHistoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public HistoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task AddRoomChatAsync(IReadOnlyList<RoomChatRecord> records, CancellationToken cancellationToken = default) =>
        InsertAsync(
            "historical_chatlogs_rooms", ["said_at", "user_ref", "room_ref", "message", "type", "whispered_user_ref", "ip_address"],
            records, r => [r.SaidAt, r.UserId, r.RoomId, r.Message, TypeName(r.Type), r.WhisperedUserId, r.IpAddress], cancellationToken);

    public Task AddInstantMessagesAsync(IReadOnlyList<InstantMessageRecord> records, CancellationToken cancellationToken = default) =>
        InsertAsync(
            "historical_chatlogs_im", ["sent_at", "from_user_ref", "to_user_ref", "message", "ip_address"],
            records, r => [r.SentAt, r.FromUserId, r.ToUserId, r.Message, r.IpAddress], cancellationToken);

    public Task AddCatalogPurchasesAsync(IReadOnlyList<CatalogPurchaseRecord> records, CancellationToken cancellationToken = default) =>
        InsertAsync(
            "historical_catalog_purchases",
            ["purchased_at", "user_ref", "page_ref", "offer_ref", "credits", "activity_points", "gift_receiver_ref", "ip_address"],
            records, r => [r.PurchasedAt, r.UserId, r.PageId, r.OfferId, r.Credits, r.ActivityPoints, r.GiftReceiverId, r.IpAddress],
            cancellationToken);

    public Task AddRoomVisitsAsync(IReadOnlyList<RoomVisitRecord> records, CancellationToken cancellationToken = default) =>
        InsertAsync(
            "historical_room_visits", ["user_ref", "room_ref", "entered_at", "left_at"],
            records, r => [r.UserId, r.RoomId, r.EnteredAt, r.LeftAt], cancellationToken);

    public async Task<HistoryRetentionDays> GetRetentionAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT catalogue_retention, chatlogs_rooms_retention, chatlogs_im_retention, trading_retention,
                   room_visits_retention, moderation_actions_retention, marketplace_sales_retention
            FROM global_historical_settings LIMIT 1;
            """, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? new HistoryRetentionDays(
                reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4), reader.GetInt32(5), reader.GetInt32(6))
            : HistoryRetentionDays.Defaults;
    }

    public async Task<int> DeleteOlderThanAsync(HistoryKind kind, int days, int limit, CancellationToken cancellationToken = default)
    {
        (string table, string column) = kind switch
        {
            HistoryKind.CatalogPurchases => ("historical_catalog_purchases", "purchased_at"),
            HistoryKind.RoomChat => ("historical_chatlogs_rooms", "said_at"),
            HistoryKind.InstantMessages => ("historical_chatlogs_im", "sent_at"),
            HistoryKind.Trading => ("historical_trading", "completed_at"),
            HistoryKind.RoomVisits => ("historical_room_visits", "entered_at"),
            HistoryKind.ModerationActions => ("historical_moderation_actions", "performed_at"),
            HistoryKind.MarketplaceSales => ("historical_marketplace_sales", "sold_at"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            $"DELETE FROM {table} WHERE {column} < UTC_TIMESTAMP() - INTERVAL @days DAY ORDER BY {column} LIMIT @limit;", connection);
        command.Parameters.AddWithValue("@days", days);
        command.Parameters.AddWithValue("@limit", limit);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string TypeName(RoomChatType type) => type switch
    {
        RoomChatType.Chat => "chat",
        RoomChatType.Shout => "shout",
        RoomChatType.Whisper => "whisper",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    private async Task InsertAsync<T>(
        string table, string[] columns, IReadOnlyList<T> records, Func<T, object?[]> valuesOf, CancellationToken cancellationToken)
    {
        if (records.Count == 0)
        {
            return;
        }

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand { Connection = connection };

        var sql = new StringBuilder($"INSERT INTO {table} ({string.Join(", ", columns)}) VALUES ");
        for (int row = 0; row < records.Count; row++)
        {
            object?[] values = valuesOf(records[row]);
            sql.Append(row == 0 ? "(" : ", (");
            for (int column = 0; column < values.Length; column++)
            {
                string name = $"@r{row}c{column}";
                sql.Append(column == 0 ? name : ", " + name);
                command.Parameters.AddWithValue(name, values[column] ?? DBNull.Value);
            }

            sql.Append(')');
        }

        command.CommandText = sql.Append(';').ToString();
        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.NoReferencedRow2)
        {
            throw new MissingReferenceException($"In the table {table} a record had refered to a row that no longer exists.", ex);
        }
    }
}
