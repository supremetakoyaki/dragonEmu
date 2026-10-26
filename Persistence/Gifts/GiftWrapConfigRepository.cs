using System.Text.Json;
using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class GiftWrapConfigRepository : IGiftWrapConfigRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GiftWrapConfigRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GiftWrapConfig?> GetAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        IReadOnlyList<int> furniDefIds;
        IReadOnlyList<int> colourValues;
        IReadOnlyList<int> ribbonIds;
        int wrapPriceCredits;

        await using (var command = new MySqlCommand(
            "SELECT ref_furni_ids, colour_array, ribbon_array, wrap_price_credits FROM definition_gift_wrap_config WHERE id = 1;",
            connection))
        await using (MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            furniDefIds = ParseIntArray(reader.GetString(0));
            colourValues = ParseIntArray(reader.GetString(1));
            ribbonIds = ParseIntArray(reader.GetString(2));
            wrapPriceCredits = reader.GetInt32(3);
        }

        IReadOnlyList<GiftWrapBox> boxes = await ResolveBoxesAsync(connection, furniDefIds, cancellationToken);

        return new GiftWrapConfig(boxes, colourValues, ribbonIds, wrapPriceCredits);
    }

    private static async Task<IReadOnlyList<GiftWrapBox>> ResolveBoxesAsync(
        MySqlConnection connection, IReadOnlyList<int> furniDefIds, CancellationToken cancellationToken)
    {
        if (furniDefIds.Count == 0)
        {
            return [];
        }

        string placeholders = string.Join(",", furniDefIds.Select((_, i) => $"@id{i}"));
        await using var command = new MySqlCommand($"SELECT id, ref_furnidata FROM definition_furniture WHERE id IN ({placeholders});", connection);
        for (int i = 0; i < furniDefIds.Count; i++)
        {
            command.Parameters.AddWithValue($"@id{i}", furniDefIds[i]);
        }

        var furnidataRefById = new Dictionary<int, int>();
        await using (MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                furnidataRefById[reader.GetInt32(0)] = reader.GetInt32(1);
            }
        }

        var boxes = new List<GiftWrapBox>();
        foreach (int furniDefId in furniDefIds)
        {
            if (furnidataRefById.TryGetValue(furniDefId, out int furnidataRef))
            {
                boxes.Add(new GiftWrapBox(furniDefId, furnidataRef));
            }
        }

        return boxes;
    }

    private static IReadOnlyList<int> ParseIntArray(string json) =>
        JsonSerializer.Deserialize<List<int>>(json) ?? [];
}
