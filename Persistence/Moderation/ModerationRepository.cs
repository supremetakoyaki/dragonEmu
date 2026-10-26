using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class ModerationRepository : IModerationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ModerationRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task BanAsync(int userId, int bannedById, bool superban, string reason, DateTime expiresAt, CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            """
            INSERT INTO users_bans (user_ref, banned_by_ref, type, reason, banned_at, expires_at)
            VALUES (@userId, @byId, @type, @reason, UTC_TIMESTAMP(), @expiresAt);
            """,
            command =>
            {
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@byId", bannedById);
                command.Parameters.AddWithValue("@type", superban ? "superban" : "ban");
                command.Parameters.AddWithValue("@reason", reason);
                command.Parameters.AddWithValue("@expiresAt", expiresAt);
            },
            cancellationToken);

    public async Task<UserBan?> GetActiveBanAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT id, reason, expires_at, type FROM users_bans
            WHERE user_ref = @userId AND expires_at > UTC_TIMESTAMP()
            ORDER BY expires_at DESC
            LIMIT 1;
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? new UserBan(reader.GetInt32(0), reader.GetString(1), DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc), reader.GetString(3) == "superban")
            : null;
    }

    public Task MuteAsync(int userId, int mutedById, DateTime expiresAt, CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            """
            INSERT INTO users_mutes (user_ref, muted_by_ref, muted_at, expires_at) VALUES (@id, @byId, UTC_TIMESTAMP(), @expiresAt)
            ON DUPLICATE KEY UPDATE muted_by_ref = VALUES(muted_by_ref), muted_at = VALUES(muted_at), expires_at = VALUES(expires_at);
            """,
            command => BindMute(command, userId, mutedById, expiresAt),
            cancellationToken);

    public Task<DateTime?> GetMuteAsync(int userId, CancellationToken cancellationToken = default) =>
        GetExpiryAsync("SELECT expires_at FROM users_mutes WHERE user_ref = @id AND expires_at > UTC_TIMESTAMP();", userId, cancellationToken);

    public async Task<bool> UnmuteAsync(int userId, CancellationToken cancellationToken = default) =>
        await ExecuteAsync(
            "DELETE FROM users_mutes WHERE user_ref = @id AND expires_at > UTC_TIMESTAMP();",
            command => command.Parameters.AddWithValue("@id", userId),
            cancellationToken) > 0;

    public Task MuteRoomAsync(int roomId, int mutedById, DateTime expiresAt, CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            """
            INSERT INTO room_mutes (room_ref, muted_by_ref, muted_at, expires_at) VALUES (@id, @byId, UTC_TIMESTAMP(), @expiresAt)
            ON DUPLICATE KEY UPDATE muted_by_ref = VALUES(muted_by_ref), muted_at = VALUES(muted_at), expires_at = VALUES(expires_at);
            """,
            command => BindMute(command, roomId, mutedById, expiresAt),
            cancellationToken);

    public Task<DateTime?> GetRoomMuteAsync(int roomId, CancellationToken cancellationToken = default) =>
        GetExpiryAsync("SELECT expires_at FROM room_mutes WHERE room_ref = @id AND expires_at > UTC_TIMESTAMP();", roomId, cancellationToken);

    public async Task<bool> UnmuteRoomAsync(int roomId, CancellationToken cancellationToken = default) =>
        await ExecuteAsync(
            "DELETE FROM room_mutes WHERE room_ref = @id AND expires_at > UTC_TIMESTAMP();",
            command => command.Parameters.AddWithValue("@id", roomId),
            cancellationToken) > 0;

    private static void BindMute(MySqlCommand command, int id, int mutedById, DateTime expiresAt)
    {
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@byId", mutedById);
        command.Parameters.AddWithValue("@expiresAt", expiresAt);
    }

    private async Task<DateTime?> GetExpiryAsync(string sql, int id, CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        return await command.ExecuteScalarAsync(cancellationToken) is DateTime expiresAt ? DateTime.SpecifyKind(expiresAt, DateTimeKind.Utc) : null;
    }

    private async Task<int> ExecuteAsync(string sql, Action<MySqlCommand> bind, CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        bind(command);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
