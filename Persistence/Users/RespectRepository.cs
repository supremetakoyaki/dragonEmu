using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class RespectRepository : IRespectRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RespectRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<RespectStanding> GetAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT respect_received, respect_day, respects_given_today, pet_respect_day, pet_respects_given_today
            FROM users
            WHERE id = @userId;
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? new RespectStanding(
                reader.GetInt32(0), reader.IsDBNull(1) ? null : reader.GetDateOnly(1), reader.GetInt32(2),
                reader.IsDBNull(3) ? null : reader.GetDateOnly(3), reader.GetInt32(4))
            : RespectStanding.None;
    }

    public async Task<RespectGiven?> TryGiveAsync(
        int giverId, int targetId, DateOnly today, int dailyGrant, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var spend = new MySqlCommand(
            """
            UPDATE users
            SET respects_given_today = IF(respect_day = @today, respects_given_today, 0) + 1,
                respect_day = @today,
                respect_given = respect_given + 1
            WHERE id = @giverId
              AND (respect_day IS NULL OR respect_day <> @today OR respects_given_today < @dailyGrant);
            """, connection, transaction))
        {
            spend.Parameters.AddWithValue("@today", today);
            spend.Parameters.AddWithValue("@giverId", giverId);
            spend.Parameters.AddWithValue("@dailyGrant", dailyGrant);
            if (await spend.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                return null;
            }
        }

        await using (var receive = new MySqlCommand(
            "UPDATE users SET respect_received = respect_received + 1 WHERE id = @targetId;", connection, transaction))
        {
            receive.Parameters.AddWithValue("@targetId", targetId);
            if (await receive.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return null;
            }
        }

        RespectGiven given;
        await using (var totals = new MySqlCommand(
            """
            SELECT (SELECT respect_given FROM users WHERE id = @giverId),
                   (SELECT respect_received FROM users WHERE id = @targetId);
            """, connection, transaction))
        {
            totals.Parameters.AddWithValue("@giverId", giverId);
            totals.Parameters.AddWithValue("@targetId", targetId);
            await using MySqlDataReader reader = await totals.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);
            given = new RespectGiven(reader.GetInt32(0), reader.GetInt32(1));
        }

        await transaction.CommitAsync(cancellationToken);
        return given;
    }
}
