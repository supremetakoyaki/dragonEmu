using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class PublicRoomFurnitureRepository : IPublicRoomFurnitureRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PublicRoomFurnitureRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<PublicRoomFurnitureItem>> GetForModelAsync(
        string heightmapLabel, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new MySqlCommand(
            """
            SELECT i.id, d.classname, i.x, i.y, i.z, i.rotation,
                   d.x_dim, d.y_dim, d.z_dim, d.is_sittable, d.is_bed, d.is_walkable, d.is_invisible
            FROM definition_public_room_items i
            INNER JOIN definition_public_room_furniture d ON d.id = i.furni_ref
            INNER JOIN definition_room_heightmap h ON h.id = i.heightmap_ref
            WHERE h.label = @label
            ORDER BY i.id;
            """,
            connection);
        command.Parameters.AddWithValue("@label", heightmapLabel);

        var items = new List<PublicRoomFurnitureItem>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PublicRoomFurnitureItem(
                Id: reader.GetInt32(0),
                Classname: reader.GetString(1),
                X: reader.GetInt32(2),
                Y: reader.GetInt32(3),
                Z: reader.GetDouble(4),
                Rotation: reader.GetInt32(5),
                XDim: reader.GetInt32(6),
                YDim: reader.GetInt32(7),
                ZDim: reader.GetDouble(8),
                IsSittable: reader.GetBoolean(9),
                IsBed: reader.GetBoolean(10),
                IsWalkable: reader.GetBoolean(11),
                IsInvisible: reader.GetBoolean(12)));
        }

        return items;
    }
}
