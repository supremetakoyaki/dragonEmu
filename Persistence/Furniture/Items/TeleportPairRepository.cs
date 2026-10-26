using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class TeleportPairRepository : ITeleportPairRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TeleportPairRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateAsync(int itemId1, int itemId2, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "INSERT INTO users_teleport_pairs (ref_item1, ref_item2) VALUES (@itemId1, @itemId2);",
            connection);
        command.Parameters.AddWithValue("@itemId1", itemId1);
        command.Parameters.AddWithValue("@itemId2", itemId2);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TeleportPair>> GetByItemIdAsync(int itemId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT id, ref_item1, ref_item2 FROM users_teleport_pairs WHERE ref_item1 = @itemId OR ref_item2 = @itemId;",
            connection);
        command.Parameters.AddWithValue("@itemId", itemId);

        var results = new List<TeleportPair>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new TeleportPair(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2)));
        }

        return results;
    }
}
