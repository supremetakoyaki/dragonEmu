using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class MessengerRepository : IMessengerRepository
{
    private const string ProfileColumns =
        "u.id, u.username, u.gender, u.figure, u.motto, u.last_login_at, (COALESCE(s.allow_following, '1') = '1') AS allows_following";

    private const string ProfileJoin = "LEFT JOIN users_settings s ON s.ref_userid = u.id";

    private readonly IDbConnectionFactory _connectionFactory;

    public MessengerRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<MessengerProfile>> GetFriendsAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            $"""
            SELECT {ProfileColumns}
            FROM users_friends f
            JOIN users u ON u.id = f.friend_ref
            {ProfileJoin}
            WHERE f.user_ref = @userId;
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);

        var friends = new List<MessengerProfile>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            friends.Add(ReadProfile(reader));
        }

        return friends;
    }

    public async Task<IReadOnlyList<int>> GetFriendIdsAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT friend_ref FROM users_friends WHERE user_ref = @userId;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        return await ReadIdsAsync(command, cancellationToken);
    }

    public async Task<MessengerProfile?> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand($"SELECT {ProfileColumns} FROM users u {ProfileJoin} WHERE u.id = @userId;", connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadProfile(reader) : null;
    }

    public async Task<bool> AreFriendsAsync(int userId, int otherUserId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT 1 FROM users_friends WHERE user_ref = @userId AND friend_ref = @otherUserId;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@otherUserId", otherUserId);
        return await command.ExecuteScalarAsync(cancellationToken) is not null;
    }

    public async Task<int> CountFriendsAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT COUNT(*) FROM users_friends WHERE user_ref = @userId;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<bool> AcceptsFriendRequestsAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT allow_friend_requests FROM users_settings WHERE ref_userid = @userId;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        return await command.ExecuteScalarAsync(cancellationToken) is not string setting || setting == "1";
    }

    public async Task<IReadOnlyList<FriendRequest>> GetRequestsAsync(int toUserId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT r.from_user_ref, u.username
            FROM users_friend_requests r
            JOIN users u ON u.id = r.from_user_ref
            WHERE r.to_user_ref = @toUserId
            ORDER BY r.created_at, r.from_user_ref;
            """, connection);
        command.Parameters.AddWithValue("@toUserId", toUserId);

        var requests = new List<FriendRequest>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            requests.Add(new FriendRequest(reader.GetInt32(0), reader.GetString(1)));
        }

        return requests;
    }

    public async Task<bool> HasRequestAsync(int fromUserId, int toUserId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT 1 FROM users_friend_requests WHERE to_user_ref = @toUserId AND from_user_ref = @fromUserId;", connection);
        command.Parameters.AddWithValue("@toUserId", toUserId);
        command.Parameters.AddWithValue("@fromUserId", fromUserId);
        return await command.ExecuteScalarAsync(cancellationToken) is not null;
    }

    public async Task<bool> AddRequestAsync(int fromUserId, int toUserId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "INSERT IGNORE INTO users_friend_requests (to_user_ref, from_user_ref) VALUES (@toUserId, @fromUserId);", connection);
        command.Parameters.AddWithValue("@toUserId", toUserId);
        command.Parameters.AddWithValue("@fromUserId", fromUserId);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> AcceptRequestAsync(int fromUserId, int toUserId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var take = new MySqlCommand(
            "DELETE FROM users_friend_requests WHERE to_user_ref = @toUserId AND from_user_ref = @fromUserId;", connection, transaction))
        {
            take.Parameters.AddWithValue("@toUserId", toUserId);
            take.Parameters.AddWithValue("@fromUserId", fromUserId);
            if (await take.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                return false;
            }
        }

        await using (var befriend = new MySqlCommand(
            """
            DELETE FROM users_friend_requests WHERE to_user_ref = @fromUserId AND from_user_ref = @toUserId;
            INSERT IGNORE INTO users_friends (user_ref, friend_ref) VALUES (@fromUserId, @toUserId), (@toUserId, @fromUserId);
            """, connection, transaction))
        {
            befriend.Parameters.AddWithValue("@toUserId", toUserId);
            befriend.Parameters.AddWithValue("@fromUserId", fromUserId);
            await befriend.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task DeclineRequestsAsync(int toUserId, IReadOnlyCollection<int> fromUserIds, CancellationToken cancellationToken = default)
    {
        if (fromUserIds.Count == 0)
        {
            return;
        }

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            $"DELETE FROM users_friend_requests WHERE to_user_ref = @toUserId AND from_user_ref IN ({IdPlaceholders(fromUserIds.Count)});", connection);
        command.Parameters.AddWithValue("@toUserId", toUserId);
        BindIds(command, fromUserIds);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeclineAllRequestsAsync(int toUserId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("DELETE FROM users_friend_requests WHERE to_user_ref = @toUserId;", connection);
        command.Parameters.AddWithValue("@toUserId", toUserId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> RemoveFriendsAsync(
        int userId, IReadOnlyCollection<int> friendIds, CancellationToken cancellationToken = default)
    {
        if (friendIds.Count == 0)
        {
            return [];
        }

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        IReadOnlyList<int> removed;
        await using (var select = new MySqlCommand(
            $"SELECT friend_ref FROM users_friends WHERE user_ref = @userId AND friend_ref IN ({IdPlaceholders(friendIds.Count)}) FOR UPDATE;",
            connection, transaction))
        {
            select.Parameters.AddWithValue("@userId", userId);
            BindIds(select, friendIds);
            removed = await ReadIdsAsync(select, cancellationToken);
        }

        if (removed.Count > 0)
        {
            await using var delete = new MySqlCommand(
                $"""
                DELETE FROM users_friends
                WHERE (user_ref = @userId AND friend_ref IN ({IdPlaceholders(removed.Count)}))
                   OR (friend_ref = @userId AND user_ref IN ({IdPlaceholders(removed.Count)}));
                """, connection, transaction);
            delete.Parameters.AddWithValue("@userId", userId);
            BindIds(delete, removed);
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return removed;
    }

    public async Task<IReadOnlyList<FriendSearchHit>> SearchAsync(
        int userId, string namePrefix, int limit, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            $"""
            SELECT {ProfileColumns}, f.friend_ref IS NOT NULL AS is_friend
            FROM users u
            {ProfileJoin}
            LEFT JOIN users_friends f ON f.user_ref = @userId AND f.friend_ref = u.id
            WHERE u.username LIKE @pattern
            ORDER BY u.username
            LIMIT @limit;
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@pattern", $"{SqlLike.Escape(namePrefix)}%");
        command.Parameters.AddWithValue("@limit", limit);

        var hits = new List<FriendSearchHit>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            hits.Add(new FriendSearchHit(ReadProfile(reader), reader.GetBoolean(7)));
        }

        return hits;
    }

    private static MessengerProfile ReadProfile(MySqlDataReader reader) => new(
        Id: reader.GetInt32(0),
        Name: reader.GetString(1),
        Gender: reader.GetString(2)[0],
        Figure: reader.GetString(3),
        Motto: reader.GetString(4),
        LastLoginAt: reader.IsDBNull(5) ? null : reader.GetDateTime(5),
        AllowsFollowing: reader.GetBoolean(6));

    private static async Task<IReadOnlyList<int>> ReadIdsAsync(MySqlCommand command, CancellationToken cancellationToken)
    {
        var ids = new List<int>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            ids.Add(reader.GetInt32(0));
        }

        return ids;
    }

    private static string IdPlaceholders(int count) => string.Join(", ", Enumerable.Range(0, count).Select(i => $"@id{i}"));

    private static void BindIds(MySqlCommand command, IReadOnlyCollection<int> ids)
    {
        int index = 0;
        foreach (int id in ids)
        {
            command.Parameters.AddWithValue($"@id{index++}", id);
        }
    }
}
