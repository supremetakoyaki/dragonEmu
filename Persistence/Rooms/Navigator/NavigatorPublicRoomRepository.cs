using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class NavigatorPublicRoomRepository : INavigatorPublicRoomRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public NavigatorPublicRoomRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<NavigatorPublicRoom>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        const string sql =
            """
            SELECT np.id, np.order_num, np.image_size, r.label, r.description, np.image, np.image_type,
                   np.room_ref, r.max_users, np.category_ref, np.club_only,
                   np.nav_index, np.visible
            FROM definition_navigator_public_rooms np
            INNER JOIN rooms r ON r.id = np.room_ref
            ORDER BY np.order_num ASC;
            """;

        var rooms = new List<NavigatorPublicRoom>();

        await using var command = new MySqlCommand(sql, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rooms.Add(new NavigatorPublicRoom(
                Id: reader.GetInt32(0),
                OrderNum: reader.GetInt32(1),
                ImageSize: reader.GetString(2) == "wide" ? NavigatorImageSize.Wide : NavigatorImageSize.Square,
                Caption: reader.GetString(3),
                Description: reader.GetString(4),
                Image: reader.GetString(5),
                ImageType: reader.GetString(6) == "external" ? NavigatorImageType.External : NavigatorImageType.Internal,
                RoomId: reader.GetInt32(7),
                MaxUsers: reader.GetInt32(8),
                CategoryRef: reader.GetInt32(9),
                ClubOnly: reader.GetBoolean(10),
                NavIndex: reader.GetInt32(11),
                Visible: reader.GetString(12) == "1"));
        }

        return rooms;
    }
}
