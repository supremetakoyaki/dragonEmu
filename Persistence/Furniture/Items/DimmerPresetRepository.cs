using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class DimmerPresetRepository : IDimmerPresetRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DimmerPresetRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<DimmerPreset>> GetPresetsAsync(int itemId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using (var create = new MySqlCommand(
            "INSERT IGNORE INTO users_items_dimmer_presets (item_ref, preset_id) VALUES (@itemId, 1), (@itemId, 2), (@itemId, 3);",
            connection))
        {
            create.Parameters.AddWithValue("@itemId", itemId);
            await create.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var command = new MySqlCommand(
            "SELECT preset_id, type, color, intensity FROM users_items_dimmer_presets WHERE item_ref = @itemId ORDER BY preset_id;",
            connection);
        command.Parameters.AddWithValue("@itemId", itemId);

        var presets = new List<DimmerPreset>(RoomDimmers.PresetCount);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            presets.Add(new DimmerPreset(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3)));
        }

        return presets;
    }

    public async Task SavePresetAsync(int itemId, DimmerPreset preset, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            INSERT INTO users_items_dimmer_presets (item_ref, preset_id, type, color, intensity)
            VALUES (@itemId, @presetId, @type, @color, @intensity)
            ON DUPLICATE KEY UPDATE type = VALUES(type), color = VALUES(color), intensity = VALUES(intensity);
            """,
            connection);
        command.Parameters.AddWithValue("@itemId", itemId);
        command.Parameters.AddWithValue("@presetId", preset.Id);
        command.Parameters.AddWithValue("@type", preset.Type);
        command.Parameters.AddWithValue("@color", preset.Color);
        command.Parameters.AddWithValue("@intensity", preset.Intensity);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
