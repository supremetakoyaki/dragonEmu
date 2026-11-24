using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class FireworkRepository : IFireworkRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public FireworkRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<FireworkDefinition?> GetAsync(int furniDefId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT furni_def_ref, price_credits, price_activity_points, activity_point_type, charge_amount, launch_duration_ms
            FROM definition_fireworks
            WHERE furni_def_ref = @furniDefId;
            """, connection);
        command.Parameters.AddWithValue("@furniDefId", furniDefId);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new FireworkDefinition(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4),
            TimeSpan.FromMilliseconds(reader.GetInt32(5)));
    }
}
