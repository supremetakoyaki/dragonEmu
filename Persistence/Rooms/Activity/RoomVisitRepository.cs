using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class RoomVisitRepository : IRoomVisitRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoomVisitRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> RecordVisitAsync(int userId, int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using (var insert = new MySqlCommand(
            """
            INSERT IGNORE INTO users_visited_rooms (user_ref, room_ref, first_visited_at, last_visited_at)
            VALUES (@userId, @roomId, UTC_TIMESTAMP(), UTC_TIMESTAMP());
            """, connection))
        {
            insert.Parameters.AddWithValue("@userId", userId);
            insert.Parameters.AddWithValue("@roomId", roomId);
            if (await insert.ExecuteNonQueryAsync(cancellationToken) > 0)
            {
                return true;
            }
        }

        await using var touch = new MySqlCommand(
            "UPDATE users_visited_rooms SET last_visited_at = UTC_TIMESTAMP() WHERE user_ref = @userId AND room_ref = @roomId;", connection);
        touch.Parameters.AddWithValue("@userId", userId);
        touch.Parameters.AddWithValue("@roomId", roomId);
        await touch.ExecuteNonQueryAsync(cancellationToken);
        return false;
    }
}
