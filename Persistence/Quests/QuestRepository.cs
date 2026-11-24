using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class QuestRepository : IQuestRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public QuestRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<QuestDefinition>> GetCampaignAsync(string campaign, int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT q.id, q.campaign, q.localization_code, q.quest_type, q.furni_classname, q.reward_activity_points,
                   q.reward_activity_point_type, q.starts_at, q.ends_at,
                   EXISTS (SELECT 1 FROM users_quest_active a WHERE a.user_ref = @userId AND a.quest_ref = q.id),
                   EXISTS (SELECT 1 FROM users_quest_completed c WHERE c.user_ref = @userId AND c.quest_ref = q.id)
            FROM definition_quests q
            WHERE q.campaign = @campaign AND q.enabled = 1
            ORDER BY q.id;
            """, connection);
        command.Parameters.AddWithValue("@campaign", campaign);
        command.Parameters.AddWithValue("@userId", userId);

        var quests = new List<QuestDefinition>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            quests.Add(new QuestDefinition(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5),
                reader.GetByte(6),
                reader.IsDBNull(7) ? null : DateTime.SpecifyKind(reader.GetDateTime(7), DateTimeKind.Utc),
                reader.IsDBNull(8) ? null : DateTime.SpecifyKind(reader.GetDateTime(8), DateTimeKind.Utc),
                reader.GetInt64(9) != 0,
                reader.GetInt64(10) != 0));
        }

        return quests;
    }

    public async Task AcceptAsync(int userId, int questId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "REPLACE INTO users_quest_active (user_ref, quest_ref, accepted_at) VALUES (@userId, @questId, UTC_TIMESTAMP());", connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@questId", questId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> CompleteAsync(int userId, int questId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var remove = new MySqlCommand(
            "DELETE FROM users_quest_active WHERE user_ref = @userId AND quest_ref = @questId;", connection, transaction);
        remove.Parameters.AddWithValue("@userId", userId);
        remove.Parameters.AddWithValue("@questId", questId);
        if (await remove.ExecuteNonQueryAsync(cancellationToken) == 0)
        {
            return false;
        }

        await using var complete = new MySqlCommand(
            "INSERT IGNORE INTO users_quest_completed (user_ref, quest_ref, completed_at) VALUES (@userId, @questId, UTC_TIMESTAMP());",
            connection, transaction);
        complete.Parameters.AddWithValue("@userId", userId);
        complete.Parameters.AddWithValue("@questId", questId);
        await complete.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }
}
