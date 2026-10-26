using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class ProfanityWordRepository : IProfanityWordRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProfanityWordRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<string>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT word FROM global_profanity_words;", connection);

        var words = new List<string>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            words.Add(reader.GetString(0));
        }

        return words;
    }
}
