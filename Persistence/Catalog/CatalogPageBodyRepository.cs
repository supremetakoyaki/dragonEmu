using System.Text.Json;
using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class CatalogPageBodyRepository : ICatalogPageBodyRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CatalogPageBodyRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CatalogPageBody?> GetByPageIdAsync(int pageId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT ref_page_id, layout_code, localization_images, localization_texts
            FROM definition_catalog_page_body
            WHERE ref_page_id = @pageId
            LIMIT 1;
            """,
            connection);
        command.Parameters.AddWithValue("@pageId", pageId);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new CatalogPageBody(
            reader.GetInt32(0),
            reader.GetString(1),
            ParseStringArray(reader.GetString(2)),
            ParseStringArray(reader.GetString(3)));
    }

    private static IReadOnlyList<string> ParseStringArray(string json) =>
        JsonSerializer.Deserialize<List<string>>(json) ?? [];
}
