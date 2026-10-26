using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class PublicRoomWarpTileRepository : IPublicRoomWarpTileRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PublicRoomWarpTileRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<PublicRoomWarpTile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT h.label, w.x, w.y,
                   (SELECT MIN(r.id) FROM rooms r WHERE r.heightmap_ref = w.target_heightmap_ref AND r.owner_ref IS NULL),
                   w.target_x, w.target_y
            FROM definition_public_room_warp_tiles w
            INNER JOIN definition_room_heightmap h ON h.id = w.heightmap_ref
            ORDER BY w.id;
            """, connection);

        var tiles = new List<PublicRoomWarpTile>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (!reader.IsDBNull(3))
            {
                tiles.Add(new PublicRoomWarpTile(
                    reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4), reader.GetInt32(5)));
            }
        }

        return tiles;
    }
}
