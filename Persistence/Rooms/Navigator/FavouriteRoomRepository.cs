using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class FavouriteRoomRepository : IFavouriteRoomRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public FavouriteRoomRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<int>> GetRoomIdsAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT room_ref FROM users_favourite_rooms WHERE user_ref = @userId ORDER BY created_at, room_ref;", connection);
        command.Parameters.AddWithValue("@userId", userId);

        var roomIds = new List<int>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            roomIds.Add(reader.GetInt32(0));
        }

        return roomIds;
    }

    public async Task<FavouriteAddResult> TryAddAsync(int userId, int roomId, int limit, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using (var insert = new MySqlCommand(
            """
            INSERT IGNORE INTO users_favourite_rooms (user_ref, room_ref)
            SELECT @userId, r.id
            FROM rooms r
            WHERE r.id = @roomId
              AND r.owner_ref IS NOT NULL
              AND (SELECT COUNT(*) FROM users_favourite_rooms f WHERE f.user_ref = @userId) < @limit;
            """, connection))
        {
            insert.Parameters.AddWithValue("@userId", userId);
            insert.Parameters.AddWithValue("@roomId", roomId);
            insert.Parameters.AddWithValue("@limit", limit);

            if (await insert.ExecuteNonQueryAsync(cancellationToken) == 1)
            {
                return FavouriteAddResult.Added;
            }
        }

        await using var count = new MySqlCommand(
            "SELECT COUNT(*) FROM users_favourite_rooms WHERE user_ref = @userId;", connection);
        count.Parameters.AddWithValue("@userId", userId);

        return Convert.ToInt32(await count.ExecuteScalarAsync(cancellationToken)) >= limit
            ? FavouriteAddResult.ListFull
            : FavouriteAddResult.Refused;
    }

    public async Task<bool> RemoveAsync(int userId, int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "DELETE FROM users_favourite_rooms WHERE user_ref = @userId AND room_ref = @roomId;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@roomId", roomId);

        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }
}
