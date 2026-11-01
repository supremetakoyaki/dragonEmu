using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class WebCommandRepository : IWebCommandRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public WebCommandRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<WebCommand>> GetPendingAsync(int limit, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT id, ref_user, command FROM web_emulator_commands ORDER BY id LIMIT @limit;", connection);
        command.Parameters.AddWithValue("@limit", limit);

        var pending = new List<WebCommand>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            pending.Add(new WebCommand(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2)));
        }

        return pending;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("DELETE FROM web_emulator_commands WHERE id = @id;", connection);
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
