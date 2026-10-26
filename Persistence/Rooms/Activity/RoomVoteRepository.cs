using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class RoomVoteRepository : IRoomVoteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoomVoteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> HasVotedAsync(int userId, int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT 1 FROM users_voted_rooms WHERE user_ref = @userId AND room_ref = @roomId LIMIT 1;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@roomId", roomId);

        return await command.ExecuteScalarAsync(cancellationToken) is not null;
    }

    public async Task<int> GetScoreAsync(int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT score FROM rooms WHERE id = @roomId;", connection);
        command.Parameters.AddWithValue("@roomId", roomId);

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null ? 0 : Convert.ToInt32(result);
    }

    public async Task<int?> VoteAsync(int userId, int roomId, int vote, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var insert = new MySqlCommand(
            "INSERT IGNORE INTO users_voted_rooms (user_ref, room_ref, vote) VALUES (@userId, @roomId, @vote);",
            connection, transaction))
        {
            insert.Parameters.AddWithValue("@userId", userId);
            insert.Parameters.AddWithValue("@roomId", roomId);
            insert.Parameters.AddWithValue("@vote", vote);

            if (await insert.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return null;
            }
        }

        await using (var update = new MySqlCommand(
            "UPDATE rooms SET score = score + @vote WHERE id = @roomId;", connection, transaction))
        {
            update.Parameters.AddWithValue("@vote", vote);
            update.Parameters.AddWithValue("@roomId", roomId);
            await update.ExecuteNonQueryAsync(cancellationToken);
        }

        int score;
        await using (var select = new MySqlCommand("SELECT score FROM rooms WHERE id = @roomId;", connection, transaction))
        {
            select.Parameters.AddWithValue("@roomId", roomId);
            score = Convert.ToInt32(await select.ExecuteScalarAsync(cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
        return score;
    }
}
