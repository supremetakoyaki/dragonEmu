using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class UserTagRepository : IUserTagRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserTagRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<string>> GetAsync(int userId, int limit, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT tag FROM users_tags WHERE user_ref = @userId ORDER BY id LIMIT @limit;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@limit", limit);

        var tags = new List<string>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tags.Add(reader.GetString(0));
        }

        return tags;
    }
}
