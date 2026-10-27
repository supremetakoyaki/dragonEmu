using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed record ServerStats(bool ServerUp, TimeSpan Uptime, int UsersOnline);

public interface IServerStatsRepository
{
    Task SaveAsync(ServerStats stats, CancellationToken cancellationToken = default);
}

public sealed class ServerStatsRepository : IServerStatsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ServerStatsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task SaveAsync(ServerStats stats, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var update = new MySqlCommand(
            "UPDATE global_server_stats SET server_up = @up, uptime_seconds = @uptime, users_online = @users, updated_at = UTC_TIMESTAMP();",
            connection);
        update.Parameters.AddWithValue("@up", stats.ServerUp ? "1" : "0");
        update.Parameters.AddWithValue("@uptime", (uint)Math.Clamp(stats.Uptime.TotalSeconds, 0, uint.MaxValue));
        update.Parameters.AddWithValue("@users", Math.Max(0, stats.UsersOnline));

        if (await update.ExecuteNonQueryAsync(cancellationToken) > 0)
        {
            return;
        }

        update.CommandText = "INSERT INTO global_server_stats (server_up, uptime_seconds, users_online, updated_at) VALUES (@up, @uptime, @users, UTC_TIMESTAMP());";
        await update.ExecuteNonQueryAsync(cancellationToken);
    }
}
