using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class FurnitureAliasRepository : IFurnitureAliasRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public FurnitureAliasRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<FurnitureAlias>> GetAllEnabledAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT furniture_classname, furniture_classname_alias FROM definition_furniture_aliases WHERE enabled = 1;", connection);

        var aliases = new List<FurnitureAlias>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            aliases.Add(new FurnitureAlias(reader.GetString(0), reader.GetString(1)));
        }

        return aliases;
    }
}
