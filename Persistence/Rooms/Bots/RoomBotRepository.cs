using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class RoomBotRepository : IRoomBotRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoomBotRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<RoomBotRecord>> GetForRoomAsync(int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var actions = new Dictionary<int, List<RoomBotActionRecord>>();
        await using (var actionQuery = new MySqlCommand(
            """
            SELECT a.bot_ref, a.id, a.trigger_type, a.trigger_data, a.responses, a.tile_distance_required
            FROM room_bots_actions a
            INNER JOIN room_bots b ON b.id = a.bot_ref
            WHERE b.room_ref = @roomId AND a.enabled = '1'
            ORDER BY a.id;
            """, connection))
        {
            actionQuery.Parameters.AddWithValue("@roomId", roomId);
            await using MySqlDataReader reader = await actionQuery.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                int botId = reader.GetInt32(0);
                if (!actions.TryGetValue(botId, out List<RoomBotActionRecord>? list))
                {
                    actions[botId] = list = [];
                }

                list.Add(new RoomBotActionRecord(
                    reader.GetInt32(1), reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetString(4), reader.GetInt32(5)));
            }
        }

        var bots = new List<RoomBotRecord>();
        await using (var botQuery = new MySqlCommand(
            """
            SELECT id, room_ref, name, gender, figure, motto, x, y, z, rotation, walkable_area, diagonal_move_allowed
            FROM room_bots
            WHERE room_ref = @roomId
            ORDER BY id;
            """, connection))
        {
            botQuery.Parameters.AddWithValue("@roomId", roomId);
            await using MySqlDataReader reader = await botQuery.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                int id = reader.GetInt32(0);
                bots.Add(new RoomBotRecord(
                    id, reader.GetInt32(1), reader.GetString(2), reader.GetString(3)[0], reader.GetString(4), reader.GetString(5),
                    reader.GetInt32(6), reader.GetInt32(7), (double)reader.GetDecimal(8), reader.GetInt32(9),
                    reader.IsDBNull(10) ? null : reader.GetString(10), reader.GetString(11) == "1",
                    actions.TryGetValue(id, out List<RoomBotActionRecord>? botActions) ? botActions : []));
            }
        }

        return bots;
    }
}
