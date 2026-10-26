using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class RoomHeightmapRepository : IRoomHeightmapRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoomHeightmapRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<RoomModel?> GetByLabelAsync(string label, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT id, label, club FROM definition_room_heightmap WHERE label = @label LIMIT 1;", connection);
        command.Parameters.AddWithValue("@label", label);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? new RoomModel(reader.GetInt32(0), reader.GetString(1), reader.GetBoolean(2))
            : null;
    }

    public async Task<int?> GetAnyRoomIdByHeightmapLabelAsync(string label, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT r.id FROM rooms r
            INNER JOIN definition_room_heightmap h ON h.id = r.heightmap_ref
            WHERE h.label = @label
            LIMIT 1;
            """, connection);
        command.Parameters.AddWithValue("@label", label);

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null ? null : Convert.ToInt32(result);
    }
}
