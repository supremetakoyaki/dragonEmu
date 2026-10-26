using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class IgnoreRepository : IIgnoreRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public IgnoreRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<(int UserId, string Username)>> GetIgnoredAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT u.id, u.username FROM users_ignores i
            INNER JOIN users u ON u.id = i.ignored_user_ref
            WHERE i.user_ref = @userId
            ORDER BY i.ignored_at, u.id;
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);

        var ignored = new List<(int, string)>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            ignored.Add((reader.GetInt32(0), reader.GetString(1)));
        }

        return ignored;
    }

    public async Task<(bool Added, IReadOnlyList<int> Dropped)> AddAsync(
        int userId, int ignoredUserId, int max, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var add = new MySqlCommand(
            "INSERT IGNORE INTO users_ignores (user_ref, ignored_user_ref, ignored_at) VALUES (@userId, @ignoredId, UTC_TIMESTAMP(3));",
            connection, transaction))
        {
            add.Parameters.AddWithValue("@userId", userId);
            add.Parameters.AddWithValue("@ignoredId", ignoredUserId);
            if (await add.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return (false, []);
            }
        }

        var dropped = new List<int>();
        await using (var oldest = new MySqlCommand(
            """
            SELECT ignored_user_ref FROM users_ignores
            WHERE user_ref = @userId AND ignored_user_ref <> @ignoredId
            ORDER BY ignored_at DESC, ignored_user_ref DESC
            LIMIT 18446744073709551615 OFFSET @keep;
            """, connection, transaction))
        {
            oldest.Parameters.AddWithValue("@userId", userId);
            oldest.Parameters.AddWithValue("@ignoredId", ignoredUserId);
            oldest.Parameters.AddWithValue("@keep", Math.Max(0, max - 1));
            await using MySqlDataReader reader = await oldest.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                dropped.Add(reader.GetInt32(0));
            }
        }

        if (dropped.Count > 0)
        {
            await using var drop = new MySqlCommand(
                $"DELETE FROM users_ignores WHERE user_ref = @userId AND ignored_user_ref IN ({string.Join(", ", dropped.Select((_, i) => $"@d{i}"))});",
                connection, transaction);
            drop.Parameters.AddWithValue("@userId", userId);
            for (int i = 0; i < dropped.Count; i++)
            {
                drop.Parameters.AddWithValue($"@d{i}", dropped[i]);
            }

            await drop.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return (true, dropped);
    }

    public async Task<bool> RemoveAsync(int userId, int ignoredUserId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "DELETE FROM users_ignores WHERE user_ref = @userId AND ignored_user_ref = @ignoredId;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@ignoredId", ignoredUserId);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
