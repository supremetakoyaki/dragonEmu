using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class NavigatorPublicRoomCategoryRepository : INavigatorPublicRoomCategoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public NavigatorPublicRoomCategoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<NavigatorPublicRoomCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        const string sql =
            """
            SELECT id, order_num, caption, image
            FROM definition_navigator_public_rooms_categories
            ORDER BY order_num ASC, id ASC;
            """;

        var categories = new List<NavigatorPublicRoomCategory>();

        await using var command = new MySqlCommand(sql, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            categories.Add(new NavigatorPublicRoomCategory(
                Id: reader.GetInt32(0),
                OrderNum: reader.GetInt32(1),
                Caption: reader.GetString(2),
                Image: reader.GetString(3)));
        }

        return categories;
    }
}
