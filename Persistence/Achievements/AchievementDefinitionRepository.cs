using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class AchievementDefinitionRepository : IAchievementDefinitionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AchievementDefinitionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<AchievementDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT a.id, a.code_base, a.max_level, a.enabled, l.level, l.threshold, l.badge_code, l.reward_activity_points
            FROM definition_achievements a
            LEFT JOIN definition_achievement_levels l ON l.achievement_ref = a.id
            ORDER BY a.id, l.level;
            """,
            connection);

        var definitions = new List<AchievementDefinition>();
        int? currentId = null;
        List<AchievementLevel> currentLevels = [];

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            int id = reader.GetInt32(0);
            if (id != currentId)
            {
                currentId = id;
                currentLevels = [];
                definitions.Add(new AchievementDefinition(id, reader.GetString(1), reader.GetInt32(2), reader.GetBoolean(3), currentLevels));
            }

            if (!reader.IsDBNull(4))
            {
                currentLevels.Add(new AchievementLevel(reader.GetInt32(4), reader.GetInt32(5), reader.GetString(6), reader.GetInt32(7)));
            }
        }

        return definitions;
    }
}
