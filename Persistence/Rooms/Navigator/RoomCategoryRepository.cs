using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class RoomCategoryRepository : IRoomCategoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoomCategoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<RoomCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT id, label, required_rights FROM definition_room_category ORDER BY id;", connection);

        var categories = new List<RoomCategory>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            categories.Add(new RoomCategory(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2)));
        }

        return categories;
    }
}
