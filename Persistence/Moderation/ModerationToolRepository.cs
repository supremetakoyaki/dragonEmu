using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class ModerationToolRepository : IModerationToolRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ModerationToolRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task RecordActionAsync(ModerationActionRecord action, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            INSERT INTO historical_moderation_actions (performed_at, moderator_ref, user_ref, room_ref, action, message, issue_message)
            VALUES (UTC_TIMESTAMP(), @moderatorId, @userId, @roomId, @action, @message, @issueMessage);
            """, connection);
        command.Parameters.AddWithValue("@moderatorId", action.ModeratorId);
        command.Parameters.AddWithValue("@userId", action.UserId);
        command.Parameters.AddWithValue("@roomId", action.RoomId);
        command.Parameters.AddWithValue("@action", action.Action switch
        {
            ModerationActionType.Caution => "caution",
            ModerationActionType.Message => "message",
            ModerationActionType.Kick => "kick",
            ModerationActionType.Ban => "ban",
            ModerationActionType.RoomCaution => "room_caution",
            ModerationActionType.RoomMessage => "room_message",
            ModerationActionType.RoomLock => "room_lock",
            ModerationActionType.RoomRename => "room_rename",
            _ => "room_kick",
        });
        command.Parameters.AddWithValue("@message", Cut(action.Message));
        command.Parameters.AddWithValue("@issueMessage", action.IssueMessage is { Length: > 0 } issue ? Cut(issue) : null);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<ModeratorUserInfo?> GetUserInfoAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT u.id, u.username, u.created_at, u.last_login_at,
                   (SELECT COUNT(*) FROM moderation_issues i WHERE i.reporter_ref = u.id),
                   (SELECT COUNT(*) FROM moderation_issues i WHERE i.reporter_ref = u.id AND i.resolution = 'abusive'),
                   (SELECT COUNT(*) FROM historical_moderation_actions a WHERE a.user_ref = u.id AND a.action = 'caution'),
                   (SELECT COUNT(*) FROM users_bans b WHERE b.user_ref = u.id)
            FROM users u
            WHERE u.id = @userId;
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new ModeratorUserInfo(
            reader.GetInt32(0), reader.GetString(1), Utc(reader.GetDateTime(2)), reader.IsDBNull(3) ? null : Utc(reader.GetDateTime(3)),
            reader.GetInt32(4), reader.GetInt32(5), reader.GetInt32(6), reader.GetInt32(7));
    }

    public Task<IReadOnlyList<RoomVisit>> GetVisitsAsync(int userId, int limit, CancellationToken cancellationToken = default) =>
        ReadVisitsAsync(userId, limit, withChatOnly: false, cancellationToken);

    public Task<IReadOnlyList<RoomVisit>> GetVisitsWithChatAsync(int userId, int limit, CancellationToken cancellationToken = default) =>
        ReadVisitsAsync(userId, limit, withChatOnly: true, cancellationToken);

    private async Task<IReadOnlyList<RoomVisit>> ReadVisitsAsync(int userId, int limit, bool withChatOnly, CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT v.room_ref, r.label, r.owner_ref IS NULL, v.entered_at, v.left_at
            FROM historical_room_visits v
            INNER JOIN rooms r ON r.id = v.room_ref
            WHERE v.user_ref = @userId
              AND (@withChatOnly = 0 OR EXISTS (
                  SELECT 1 FROM historical_chatlogs_rooms c
                  WHERE c.room_ref = v.room_ref AND c.said_at BETWEEN v.entered_at AND v.left_at))
            ORDER BY v.entered_at DESC
            LIMIT @limit;
            """, connection);
        command.Parameters.AddWithValue("@withChatOnly", withChatOnly);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@limit", limit);

        var visits = new List<RoomVisit>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            visits.Add(new RoomVisit(reader.GetInt32(0), reader.GetString(1), reader.GetBoolean(2), Utc(reader.GetDateTime(3)), Utc(reader.GetDateTime(4))));
        }

        return visits;
    }

    public async Task<IReadOnlyList<ChatlogLine>> GetRoomChatAsync(
        int roomId, DateTime? from, DateTime? to, int limit, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT c.said_at, c.user_ref, COALESCE(u.username, ''), c.message, c.type, COALESCE(w.username, '')
            FROM historical_chatlogs_rooms c
            LEFT JOIN users u ON u.id = c.user_ref
            LEFT JOIN users w ON w.id = c.whispered_user_ref
            WHERE c.room_ref = @roomId AND (@from IS NULL OR c.said_at >= @from) AND (@to IS NULL OR c.said_at <= @to)
            ORDER BY c.said_at DESC, c.id DESC
            LIMIT @limit;
            """, connection);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@from", from);
        command.Parameters.AddWithValue("@to", to);
        command.Parameters.AddWithValue("@limit", limit);

        var lines = new List<ChatlogLine>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            string message = reader.GetString(4) == "whisper" ? $"(whisper to {reader.GetString(5)}) {reader.GetString(3)}" : reader.GetString(3);
            lines.Add(new ChatlogLine(Utc(reader.GetDateTime(0)), reader.GetInt32(1), reader.GetString(2), message));
        }

        lines.Reverse();
        return lines;
    }

    private static DateTime Utc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static string Cut(string text) => text.Length <= 500 ? text : text[..500];
}
