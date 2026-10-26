using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class AvatarEffectRepository : IAvatarEffectRepository
{
    private const string DropUsedUp = "DELETE FROM users_avatar_fx WHERE user_ref = @userId AND quantity = 0 AND expires_at IS NULL";

    private readonly IDbConnectionFactory _connectionFactory;

    public AvatarEffectRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<AvatarEffect>> GetAsync(int userId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using (var expire = new MySqlCommand(
            "UPDATE users_avatar_fx SET expires_at = NULL WHERE user_ref = @userId AND expires_at <= @now;", connection))
        {
            expire.Parameters.AddWithValue("@userId", userId);
            expire.Parameters.AddWithValue("@now", utcNow);

            if (await expire.ExecuteNonQueryAsync(cancellationToken) > 0)
            {
                await using var drop = new MySqlCommand($"{DropUsedUp};", connection);
                drop.Parameters.AddWithValue("@userId", userId);
                await drop.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        await using var select = new MySqlCommand(
            "SELECT effect_id, duration, quantity, expires_at FROM users_avatar_fx WHERE user_ref = @userId ORDER BY effect_id;", connection);
        select.Parameters.AddWithValue("@userId", userId);

        var effects = new List<AvatarEffect>();
        await using MySqlDataReader reader = await select.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            effects.Add(MapRow(reader));
        }

        return effects;
    }

    public async Task AddAsync(int userId, int effectId, int duration, int quantity, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            INSERT INTO users_avatar_fx (user_ref, effect_id, duration, quantity)
            VALUES (@userId, @effectId, @duration, @quantity) AS added
            ON DUPLICATE KEY UPDATE quantity = users_avatar_fx.quantity + added.quantity;
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@effectId", effectId);
        command.Parameters.AddWithValue("@duration", duration);
        command.Parameters.AddWithValue("@quantity", quantity);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<AvatarEffect?> TryAddRunningAsync(int userId, int effectId, int duration, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new MySqlCommand(
            """
            INSERT IGNORE INTO users_avatar_fx (user_ref, effect_id, duration, quantity, expires_at)
            VALUES (@userId, @effectId, @duration, 0, DATE_ADD(@now, INTERVAL @duration SECOND));
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@effectId", effectId);
        command.Parameters.AddWithValue("@duration", duration);
        command.Parameters.AddWithValue("@now", utcNow);

        return await command.ExecuteNonQueryAsync(cancellationToken) == 1
            ? new AvatarEffect(effectId, duration, Quantity: 0, ExpiresAt: utcNow.AddSeconds(duration))
            : null;
    }

    public async Task<AvatarEffect?> TryActivateAsync(int userId, int effectId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using (var activate = new MySqlCommand(
            """
            UPDATE users_avatar_fx
            SET quantity = quantity - 1, expires_at = DATE_ADD(@now, INTERVAL duration SECOND)
            WHERE user_ref = @userId AND effect_id = @effectId AND quantity > 0 AND expires_at IS NULL;
            """, connection))
        {
            activate.Parameters.AddWithValue("@userId", userId);
            activate.Parameters.AddWithValue("@effectId", effectId);
            activate.Parameters.AddWithValue("@now", utcNow);

            if (await activate.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                return null;
            }
        }

        await using var select = new MySqlCommand(
            "SELECT effect_id, duration, quantity, expires_at FROM users_avatar_fx WHERE user_ref = @userId AND effect_id = @effectId;", connection);
        select.Parameters.AddWithValue("@userId", userId);
        select.Parameters.AddWithValue("@effectId", effectId);

        await using MySqlDataReader reader = await select.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapRow(reader) : null;
    }

    public async Task<bool> TryExpireAsync(int userId, int effectId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using (var expire = new MySqlCommand(
            "UPDATE users_avatar_fx SET expires_at = NULL WHERE user_ref = @userId AND effect_id = @effectId AND expires_at <= @now;", connection))
        {
            expire.Parameters.AddWithValue("@userId", userId);
            expire.Parameters.AddWithValue("@effectId", effectId);
            expire.Parameters.AddWithValue("@now", utcNow);

            if (await expire.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                return false;
            }
        }

        await using var drop = new MySqlCommand($"{DropUsedUp} AND effect_id = @effectId;", connection);
        drop.Parameters.AddWithValue("@userId", userId);
        drop.Parameters.AddWithValue("@effectId", effectId);
        await drop.ExecuteNonQueryAsync(cancellationToken);
        return true;
    }

    public async Task<bool> IsRunningAsync(int userId, int effectId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT 1 FROM users_avatar_fx WHERE user_ref = @userId AND effect_id = @effectId AND expires_at > @now;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@effectId", effectId);
        command.Parameters.AddWithValue("@now", utcNow);

        return await command.ExecuteScalarAsync(cancellationToken) is not null;
    }

    private static AvatarEffect MapRow(MySqlDataReader reader) => new(
        EffectId: reader.GetInt32(0),
        Duration: reader.GetInt32(1),
        Quantity: reader.GetInt32(2),
        ExpiresAt: reader.IsDBNull(3) ? null : DateTime.SpecifyKind(reader.GetDateTime(3), DateTimeKind.Utc));
}
