using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class TradeRepository : ITradeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TradeRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int?> CompleteAsync(CompletedTrade trade, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        if (!await GiveAsync(connection, transaction, trade.User1, trade.User2.UserId, cancellationToken)
            || !await GiveAsync(connection, transaction, trade.User2, trade.User1.UserId, cancellationToken))
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        int tradeId;
        await using (var record = new MySqlCommand(
            """
            INSERT INTO historical_trading (completed_at, room_ref, user1_ref, user1_ip, user2_ref, user2_ip)
            VALUES (UTC_TIMESTAMP(), @roomId, @user1Id, @user1Ip, @user2Id, @user2Ip);
            """, connection, transaction))
        {
            record.Parameters.AddWithValue("@roomId", trade.RoomId);
            record.Parameters.AddWithValue("@user1Id", trade.User1.UserId);
            record.Parameters.AddWithValue("@user1Ip", trade.User1.IpAddress);
            record.Parameters.AddWithValue("@user2Id", trade.User2.UserId);
            record.Parameters.AddWithValue("@user2Ip", trade.User2.IpAddress);
            await record.ExecuteNonQueryAsync(cancellationToken);
            tradeId = (int)record.LastInsertedId;
        }

        await RecordItemsAsync(connection, transaction, tradeId, trade.User1, cancellationToken);
        await RecordItemsAsync(connection, transaction, tradeId, trade.User2, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return tradeId;
    }

    private static async Task<bool> GiveAsync(
        MySqlConnection connection, MySqlTransaction transaction, CompletedTradeSide giver, int receiverId, CancellationToken cancellationToken)
    {
        if (giver.ItemIds.Count == 0)
        {
            return true;
        }

        await using var give = new MySqlCommand(
            $"""
            UPDATE users_items
            SET user_ref = @receiverId
            WHERE id IN ({IdPlaceholders(giver.ItemIds.Count)})
              AND user_ref = @giverId AND room_ref IS NULL AND expires_at IS NULL AND NOT EXISTS (SELECT 1 FROM marketplace_offers m WHERE m.item_ref = users_items.id)
              AND ref_furni_def IN (SELECT id FROM definition_furniture WHERE allow_trade = 1);
            """, connection, transaction);
        give.Parameters.AddWithValue("@receiverId", receiverId);
        give.Parameters.AddWithValue("@giverId", giver.UserId);
        BindIds(give, giver.ItemIds);
        return await give.ExecuteNonQueryAsync(cancellationToken) == giver.ItemIds.Count;
    }

    private static async Task RecordItemsAsync(
        MySqlConnection connection, MySqlTransaction transaction, int tradeId, CompletedTradeSide giver, CancellationToken cancellationToken)
    {
        if (giver.ItemIds.Count == 0)
        {
            return;
        }

        await using var record = new MySqlCommand(
            $"""
            INSERT INTO historical_trading_items (trade_ref, item_ref, from_user_ref, furni_def_ref, extra_data)
            SELECT @tradeId, i.id, @giverId, i.ref_furni_def, i.extra_data
            FROM users_items i
            WHERE i.id IN ({IdPlaceholders(giver.ItemIds.Count)});
            """, connection, transaction);
        record.Parameters.AddWithValue("@tradeId", tradeId);
        record.Parameters.AddWithValue("@giverId", giver.UserId);
        BindIds(record, giver.ItemIds);
        await record.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string IdPlaceholders(int count) => string.Join(", ", Enumerable.Range(0, count).Select(i => $"@id{i}"));

    private static void BindIds(MySqlCommand command, IReadOnlyCollection<int> ids)
    {
        int index = 0;
        foreach (int id in ids)
        {
            command.Parameters.AddWithValue($"@id{index++}", id);
        }
    }
}
