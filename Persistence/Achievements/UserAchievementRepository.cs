using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class UserAchievementRepository : IUserAchievementRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserAchievementRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AchievementProgress> GetProgressAsync(int userId, int achievementId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT counter, current_level FROM users_achievement_progress WHERE user_ref = @userId AND achievement_ref = @achievementId;",
            connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@achievementId", achievementId);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new AchievementProgress(achievementId, reader.GetInt32(0), reader.GetInt32(1));
        }

        return new AchievementProgress(achievementId, 0, 0);
    }

    public async Task<IReadOnlyList<(int AchievementId, AchievementProgress Progress)>> GetAllAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT achievement_ref, counter, current_level FROM users_achievement_progress WHERE user_ref = @userId AND counter > 0;",
            connection);
        command.Parameters.AddWithValue("@userId", userId);

        var results = new List<(int, AchievementProgress)>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            int achievementId = reader.GetInt32(0);
            results.Add((achievementId, new AchievementProgress(achievementId, reader.GetInt32(1), reader.GetInt32(2))));
        }

        return results;
    }

    public async Task SetProgressAsync(int userId, int achievementId, int counter, int currentLevel, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            INSERT INTO users_achievement_progress (user_ref, achievement_ref, counter, current_level, updated_at)
            VALUES (@userId, @achievementId, @counter, @currentLevel, UTC_TIMESTAMP())
            ON DUPLICATE KEY UPDATE counter = @counter, current_level = @currentLevel, updated_at = UTC_TIMESTAMP();
            """,
            connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@achievementId", achievementId);
        command.Parameters.AddWithValue("@counter", counter);
        command.Parameters.AddWithValue("@currentLevel", currentLevel);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
