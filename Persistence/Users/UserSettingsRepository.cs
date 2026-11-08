using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class UserSettingsRepository : IUserSettingsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserSettingsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserSettings> GetOrCreateAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using (var insert = new MySqlCommand(
            "INSERT IGNORE INTO users_settings (ref_userid) VALUES (@userId);", connection))
        {
            insert.Parameters.AddWithValue("@userId", userId);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var select = new MySqlCommand(
            "SELECT sound_volume, home_room, preferred_language FROM users_settings WHERE ref_userid = @userId;", connection);
        select.Parameters.AddWithValue("@userId", userId);

        await using MySqlDataReader reader = await select.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new UserSettings(reader.GetInt32(0), reader.GetInt32(1), LanguageExtensions.FromCode(reader.GetString(2)) ?? Language.En);
    }

    public async Task<Language> GetLanguageAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT preferred_language FROM users_settings WHERE ref_userid = @userId;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        return LanguageExtensions.FromCode(await command.ExecuteScalarAsync(cancellationToken) as string) ?? Language.En;
    }

    public async Task SetSoundVolumeAsync(int userId, int soundVolume, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            INSERT INTO users_settings (ref_userid, sound_volume) VALUES (@userId, @volume)
            ON DUPLICATE KEY UPDATE sound_volume = @volume;
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@volume", soundVolume);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SetHomeRoomAsync(int userId, int homeRoom, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            INSERT INTO users_settings (ref_userid, home_room) VALUES (@userId, @homeRoom)
            ON DUPLICATE KEY UPDATE home_room = @homeRoom;
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@homeRoom", homeRoom);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
