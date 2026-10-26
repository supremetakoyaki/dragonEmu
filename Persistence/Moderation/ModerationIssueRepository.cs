using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class ModerationIssueRepository : IModerationIssueRepository
{
    private const string Select =
        """
        SELECT i.id, i.state, i.category, i.reporter_ref, COALESCE(reporter.username, ''), i.reported_ref, COALESCE(reported.username, ''),
               i.room_ref, COALESCE(r.label, ''), r.owner_ref IS NULL, i.message, i.picker_ref, COALESCE(picker.username, ''), i.created_at
        FROM moderation_issues i
        LEFT JOIN users reporter ON reporter.id = i.reporter_ref
        LEFT JOIN users reported ON reported.id = i.reported_ref
        LEFT JOIN users picker ON picker.id = i.picker_ref
        LEFT JOIN rooms r ON r.id = i.room_ref
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public ModerationIssueRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ModerationIssue>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand($"{Select} WHERE i.state IN ('open', 'picked') ORDER BY i.id;", connection);
        return await ReadAsync(command, cancellationToken);
    }

    public async Task<ModerationIssue> CreateAsync(
        int category, int reporterId, int? reportedId, int? roomId, string message, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        int id;
        await using (var insert = new MySqlCommand(
            """
            INSERT INTO moderation_issues (state, category, reporter_ref, reported_ref, room_ref, message, created_at)
            VALUES ('open', @category, @reporterId, @reportedId, @roomId, @message, UTC_TIMESTAMP());
            """, connection))
        {
            insert.Parameters.AddWithValue("@category", category);
            insert.Parameters.AddWithValue("@reporterId", reporterId);
            insert.Parameters.AddWithValue("@reportedId", reportedId);
            insert.Parameters.AddWithValue("@roomId", roomId);
            insert.Parameters.AddWithValue("@message", message);
            await insert.ExecuteNonQueryAsync(cancellationToken);
            id = (int)insert.LastInsertedId;
        }

        await using var select = new MySqlCommand($"{Select} WHERE i.id = @id;", connection);
        select.Parameters.AddWithValue("@id", id);
        return (await ReadAsync(select, cancellationToken)).Single();
    }

    public Task<bool> TryPickAsync(int issueId, int moderatorId, CancellationToken cancellationToken = default) =>
        UpdateAsync(
            "UPDATE moderation_issues SET state = 'picked', picker_ref = @moderatorId, picked_at = UTC_TIMESTAMP() WHERE id = @id AND state = 'open';",
            issueId, moderatorId, resolution: null, cancellationToken);

    public Task<bool> TryReleaseAsync(int issueId, int moderatorId, CancellationToken cancellationToken = default) =>
        UpdateAsync(
            """
            UPDATE moderation_issues SET state = 'open', picker_ref = NULL, picked_at = NULL
            WHERE id = @id AND state = 'picked' AND picker_ref = @moderatorId;
            """,
            issueId, moderatorId, resolution: null, cancellationToken);

    public Task<bool> TryCloseAsync(int issueId, int moderatorId, IssueResolution resolution, CancellationToken cancellationToken = default) =>
        UpdateAsync(
            """
            UPDATE moderation_issues SET state = 'closed', resolution = @resolution, closed_at = UTC_TIMESTAMP()
            WHERE id = @id AND state = 'picked' AND picker_ref = @moderatorId;
            """,
            issueId, moderatorId, resolution switch
            {
                IssueResolution.Useless => "useless",
                IssueResolution.Abusive => "abusive",
                _ => "resolved",
            },
            cancellationToken);

    public async Task<IReadOnlyList<int>> DeleteOpenByReporterAsync(int reporterId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        var ids = new List<int>();
        await using (var select = new MySqlCommand(
            "SELECT id FROM moderation_issues WHERE reporter_ref = @reporterId AND state = 'open' FOR UPDATE;", connection, transaction))
        {
            select.Parameters.AddWithValue("@reporterId", reporterId);
            await using MySqlDataReader reader = await select.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                ids.Add(reader.GetInt32(0));
            }
        }

        await using (var delete = new MySqlCommand(
            "DELETE FROM moderation_issues WHERE reporter_ref = @reporterId AND state = 'open';", connection, transaction))
        {
            delete.Parameters.AddWithValue("@reporterId", reporterId);
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return ids;
    }

    private async Task<bool> UpdateAsync(string sql, int issueId, int moderatorId, string? resolution, CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", issueId);
        command.Parameters.AddWithValue("@moderatorId", moderatorId);
        command.Parameters.AddWithValue("@resolution", resolution);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private static async Task<IReadOnlyList<ModerationIssue>> ReadAsync(MySqlCommand command, CancellationToken cancellationToken)
    {
        var issues = new List<ModerationIssue>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            issues.Add(new ModerationIssue(
                reader.GetInt32(0),
                reader.GetString(1) switch { "picked" => IssueState.Picked, "closed" => IssueState.Closed, _ => IssueState.Open },
                reader.GetInt32(2),
                reader.GetInt32(3), reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetInt32(5), reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetInt32(7), reader.GetString(8), !reader.IsDBNull(7) && reader.GetBoolean(9),
                reader.GetString(10),
                reader.IsDBNull(11) ? null : reader.GetInt32(11), reader.GetString(12),
                DateTime.SpecifyKind(reader.GetDateTime(13), DateTimeKind.Utc)));
        }

        return issues;
    }
}
