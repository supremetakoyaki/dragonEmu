using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class FuseRepository : IFuseRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public FuseRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<(string Label, string Requirement)>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT label, requirement FROM definition_fuse;", connection);

        var fuses = new List<(string, string)>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            fuses.Add((reader.GetString(0), reader.GetString(1)));
        }

        return fuses;
    }
}
