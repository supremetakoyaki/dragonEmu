using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class UserBadgeRepository : IUserBadgeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserBadgeRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> HasBadgeAsync(int userId, string badgeCode, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT 1 FROM users_badges WHERE user_ref = @userId AND badge_code = @badgeCode LIMIT 1;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@badgeCode", badgeCode);

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null;
    }

    public async Task<bool> AwardAsync(int userId, string badgeCode, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "INSERT IGNORE INTO users_badges (user_ref, badge_code) VALUES (@userId, @badgeCode);", connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@badgeCode", badgeCode);

        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<int?> ReplaceAsync(int userId, string oldBadgeCode, string newBadgeCode, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        int? slot;
        await using (var read = new MySqlCommand(
            "SELECT slot FROM users_badges WHERE user_ref = @userId AND badge_code = @oldCode FOR UPDATE;", connection, transaction))
        {
            read.Parameters.AddWithValue("@userId", userId);
            read.Parameters.AddWithValue("@oldCode", oldBadgeCode);
            slot = await read.ExecuteScalarAsync(cancellationToken) is { } value and not DBNull ? Convert.ToInt32(value) : null;
        }

        await using (var remove = new MySqlCommand(
            "DELETE FROM users_badges WHERE user_ref = @userId AND badge_code = @oldCode;", connection, transaction))
        {
            remove.Parameters.AddWithValue("@userId", userId);
            remove.Parameters.AddWithValue("@oldCode", oldBadgeCode);
            await remove.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var award = new MySqlCommand(
            """
            INSERT INTO users_badges (user_ref, badge_code, slot) VALUES (@userId, @newCode, @slot)
            ON DUPLICATE KEY UPDATE slot = COALESCE(VALUES(slot), slot);
            """, connection, transaction))
        {
            award.Parameters.AddWithValue("@userId", userId);
            award.Parameters.AddWithValue("@newCode", newBadgeCode);
            award.Parameters.AddWithValue("@slot", slot);
            await award.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return slot;
    }

    public async Task RemoveAsync(int userId, string badgeCode, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "DELETE FROM users_badges WHERE user_ref = @userId AND badge_code = @badgeCode;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@badgeCode", badgeCode);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserBadge>> GetAllAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT badge_code, slot, awarded_at FROM users_badges WHERE user_ref = @userId ORDER BY awarded_at;", connection);
        command.Parameters.AddWithValue("@userId", userId);

        var badges = new List<UserBadge>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            badges.Add(new UserBadge(reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetInt32(1), reader.GetDateTime(2)));
        }

        return badges;
    }

    public async Task<IReadOnlyList<(int Slot, string BadgeCode)>> GetWornAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT slot, badge_code FROM users_badges WHERE user_ref = @userId AND slot IS NOT NULL ORDER BY slot;", connection);
        command.Parameters.AddWithValue("@userId", userId);

        var worn = new List<(int Slot, string BadgeCode)>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            worn.Add((reader.GetInt32(0), reader.GetString(1)));
        }

        return worn;
    }

    public async Task SetWornAsync(
        int userId, IReadOnlyList<(int Slot, string BadgeCode)> worn, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var takeOff = new MySqlCommand("UPDATE users_badges SET slot = NULL WHERE user_ref = @userId;", connection, transaction))
        {
            takeOff.Parameters.AddWithValue("@userId", userId);
            await takeOff.ExecuteNonQueryAsync(cancellationToken);
        }

        if (worn.Count > 0)
        {
            await using var putOn = new MySqlCommand(
                $"""
                UPDATE users_badges b
                JOIN (VALUES {string.Join(", ", worn.Select((_, i) => $"ROW(@code{i}, @slot{i})"))}) AS w (badge_code, slot)
                    ON w.badge_code = b.badge_code
                SET b.slot = w.slot
                WHERE b.user_ref = @userId;
                """, connection, transaction);
            putOn.Parameters.AddWithValue("@userId", userId);
            for (int i = 0; i < worn.Count; i++)
            {
                putOn.Parameters.AddWithValue($"@code{i}", worn[i].BadgeCode);
                putOn.Parameters.AddWithValue($"@slot{i}", worn[i].Slot);
            }

            await putOn.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
