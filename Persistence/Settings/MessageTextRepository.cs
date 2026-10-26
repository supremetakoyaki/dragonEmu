using MySqlConnector;

namespace DragonEmu.Persistence;

public interface IMessageTextRepository
{
    Task<IReadOnlyDictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default);
}

public sealed class MessageTextRepository : IMessageTextRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MessageTextRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT message_key, message_text FROM global_message_texts;", connection);

        var texts = new Dictionary<string, string>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            texts[reader.GetString(0)] = reader.GetString(1);
        }

        return texts;
    }
}
