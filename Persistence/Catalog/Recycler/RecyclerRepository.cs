using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class RecyclerRepository : IRecyclerRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RecyclerRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DateTime?> GetLastRecycledAtAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT last_recycled_at FROM users WHERE id = @userId;", connection);
        command.Parameters.AddWithValue("@userId", userId);

        return await command.ExecuteScalarAsync(cancellationToken) is DateTime last ? DateTime.SpecifyKind(last, DateTimeKind.Utc) : null;
    }

    public async Task<int?> TryRecycleAsync(RecycleRequest request, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var cooldown = new MySqlCommand(
            """
            UPDATE users SET last_recycled_at = @now
            WHERE id = @userId AND (last_recycled_at IS NULL OR last_recycled_at <= DATE_SUB(@now, INTERVAL @cooldown SECOND));
            """, connection, transaction))
        {
            cooldown.Parameters.AddWithValue("@userId", request.UserId);
            cooldown.Parameters.AddWithValue("@now", request.UtcNow);
            cooldown.Parameters.AddWithValue("@cooldown", request.CooldownSeconds);

            if (await cooldown.ExecuteNonQueryAsync(cancellationToken) != 1)
            {
                await transaction.RollbackAsync(cancellationToken);
                return null;
            }
        }

        await using (var recycle = new MySqlCommand(
            $"""
            DELETE i FROM users_items i
            JOIN definition_furniture d ON d.id = i.ref_furni_def
            WHERE i.user_ref = @userId AND i.room_ref IS NULL AND d.allow_recycle = 1 AND i.expires_at IS NULL
              AND NOT EXISTS (SELECT 1 FROM marketplace_offers m WHERE m.item_ref = i.id)
              AND i.id IN ({string.Join(", ", request.ItemIds.Select((_, index) => $"@item{index}"))});
            """, connection, transaction))
        {
            recycle.Parameters.AddWithValue("@userId", request.UserId);
            int index = 0;
            foreach (int itemId in request.ItemIds)
            {
                recycle.Parameters.AddWithValue($"@item{index++}", itemId);
            }

            if (await recycle.ExecuteNonQueryAsync(cancellationToken) != request.ItemIds.Count)
            {
                await transaction.RollbackAsync(cancellationToken);
                return null;
            }
        }

        int boxId;
        await using (var box = new MySqlCommand(
            "INSERT INTO users_items (user_ref, ref_furni_def, extra_data, seen) VALUES (@userId, @boxDefId, @extraData, 0);",
            connection, transaction))
        {
            box.Parameters.AddWithValue("@userId", request.UserId);
            box.Parameters.AddWithValue("@boxDefId", request.PrizeBoxFurniDefId);
            box.Parameters.AddWithValue("@extraData", request.PrizeBoxExtraData);
            await box.ExecuteNonQueryAsync(cancellationToken);
            boxId = (int)box.LastInsertedId;
        }

        await using (var prize = new MySqlCommand(
            "INSERT INTO users_gifts (ref_item, sender_ref, contents) VALUES (@boxId, NULL, @contents);", connection, transaction))
        {
            prize.Parameters.AddWithValue("@boxId", boxId);
            prize.Parameters.AddWithValue("@contents", UserGiftRepository.SerializeContents([request.Prize]));
            await prize.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return boxId;
    }
}
