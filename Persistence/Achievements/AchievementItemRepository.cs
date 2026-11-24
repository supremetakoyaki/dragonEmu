using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class AchievementItemRepository : IAchievementItemRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AchievementItemRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> TryCountAsync(int userId, string codeBase, int itemId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            INSERT IGNORE INTO users_achievement_items (user_ref, achievement_ref, item_ref)
            SELECT @userId, a.id, @itemId FROM definition_achievements a WHERE a.code_base = @codeBase;
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@itemId", itemId);
        command.Parameters.AddWithValue("@codeBase", codeBase);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }
}
