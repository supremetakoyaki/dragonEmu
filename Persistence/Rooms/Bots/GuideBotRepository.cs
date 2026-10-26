using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed record GuideBotRecord(string Name, char Gender, string Figure, string Motto, IReadOnlyList<RoomBotActionRecord> Actions);

public interface IGuideBotRepository
{
    Task<GuideBotRecord?> GetAsync(CancellationToken cancellationToken = default);
}

public sealed class GuideBotRepository : IGuideBotRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GuideBotRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GuideBotRecord?> GetAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        string name, figure, motto;
        char gender;
        await using (var data = new MySqlCommand(
            "SELECT bot_name, bot_gender, bot_figure, bot_motto FROM guide_bot_data LIMIT 1;", connection))
        {
            await using MySqlDataReader reader = await data.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            (name, gender, figure, motto) = (reader.GetString(0), reader.GetString(1)[0], reader.GetString(2), reader.GetString(3));
        }

        var actions = new List<RoomBotActionRecord>();
        await using var command = new MySqlCommand(
            """
            SELECT id, trigger_type, CAST(trigger_data AS CHAR), CAST(responses AS CHAR), tile_distance_required
            FROM guide_bot_actions
            WHERE enabled = '1'
            ORDER BY id;
            """, connection);
        await using MySqlDataReader actionReader = await command.ExecuteReaderAsync(cancellationToken);
        while (await actionReader.ReadAsync(cancellationToken))
        {
            actions.Add(new RoomBotActionRecord(
                actionReader.GetInt32(0), actionReader.GetString(1), actionReader.IsDBNull(2) ? null : actionReader.GetString(2),
                actionReader.GetString(3), actionReader.GetInt32(4)));
        }

        return new GuideBotRecord(name, gender, figure, motto, actions);
    }
}
