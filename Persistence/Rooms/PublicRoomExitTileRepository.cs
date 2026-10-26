using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class PublicRoomExitTileRepository : IPublicRoomExitTileRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PublicRoomExitTileRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<PublicRoomExitTile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT h.label, e.x, e.y FROM definition_public_room_exit_tiles e
            INNER JOIN definition_room_heightmap h ON h.id = e.heightmap_ref
            ORDER BY e.id;
            """, connection);

        var tiles = new List<PublicRoomExitTile>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tiles.Add(new PublicRoomExitTile(reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2)));
        }

        return tiles;
    }
}
