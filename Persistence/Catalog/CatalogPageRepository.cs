using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class CatalogPageRepository : ICatalogPageRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CatalogPageRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<CatalogPageSummary>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT id, parent_id, color, icon, display_name, coming_soon, enabled, min_rank
            FROM definition_catalog_page_header
            ORDER BY order_num ASC;
            """,
            connection);

        var pages = new List<CatalogPageSummary>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            pages.Add(new CatalogPageSummary(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetString(4),
                reader.GetBoolean(5),
                reader.GetBoolean(6),
                reader.GetInt32(7)));
        }

        return pages;
    }
}
