using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class RoomRightsRepository : IRoomRightsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoomRightsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> HasRightsAsync(int userId, int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT 1 FROM users_room_rights WHERE user_ref = @userId AND room_ref = @roomId LIMIT 1;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@roomId", roomId);

        return await command.ExecuteScalarAsync(cancellationToken) is not null;
    }

    public async Task<IReadOnlyList<RoomRightsHolder>> GetHoldersAsync(int roomId, int limit, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT r.user_ref, u.username FROM users_room_rights r
            INNER JOIN users u ON u.id = r.user_ref
            WHERE r.room_ref = @roomId
            ORDER BY r.granted_at DESC
            LIMIT @limit;
            """, connection);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@limit", limit);

        var holders = new List<RoomRightsHolder>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            holders.Add(new RoomRightsHolder(reader.GetInt32(0), reader.GetString(1)));
        }

        return holders;
    }

    public async Task<int> GetHolderCountAsync(int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT COUNT(*) FROM users_room_rights WHERE room_ref = @roomId;", connection);
        command.Parameters.AddWithValue("@roomId", roomId);

        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<bool> GrantAsync(int userId, int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "INSERT IGNORE INTO users_room_rights (user_ref, room_ref) VALUES (@userId, @roomId);", connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@roomId", roomId);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> RevokeAsync(int userId, int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "DELETE FROM users_room_rights WHERE user_ref = @userId AND room_ref = @roomId;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@roomId", roomId);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IReadOnlyList<int>> RevokeAllAsync(int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        var userIds = new List<int>();
        await using (var select = new MySqlCommand(
            "SELECT user_ref FROM users_room_rights WHERE room_ref = @roomId FOR UPDATE;", connection, transaction))
        {
            select.Parameters.AddWithValue("@roomId", roomId);
            await using MySqlDataReader reader = await select.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                userIds.Add(reader.GetInt32(0));
            }
        }

        await using (var delete = new MySqlCommand("DELETE FROM users_room_rights WHERE room_ref = @roomId;", connection, transaction))
        {
            delete.Parameters.AddWithValue("@roomId", roomId);
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return userIds;
    }

    public async Task BanAsync(int userId, int roomId, int bannedById, int seconds, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            DELETE FROM users_room_bans WHERE expires_at <= UTC_TIMESTAMP();
            INSERT INTO users_room_bans (user_ref, room_ref, banned_by_ref, banned_at, expires_at)
            VALUES (@userId, @roomId, @bannedById, UTC_TIMESTAMP(), UTC_TIMESTAMP() + INTERVAL @seconds SECOND)
            ON DUPLICATE KEY UPDATE banned_by_ref = VALUES(banned_by_ref), banned_at = VALUES(banned_at), expires_at = VALUES(expires_at);
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@bannedById", bannedById);
        command.Parameters.AddWithValue("@seconds", seconds);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> IsBannedAsync(int userId, int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT 1 FROM users_room_bans WHERE user_ref = @userId AND room_ref = @roomId AND expires_at > UTC_TIMESTAMP() LIMIT 1;",
            connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@roomId", roomId);

        return await command.ExecuteScalarAsync(cancellationToken) is not null;
    }
}
