using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class WardrobeRepository : IWardrobeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public WardrobeRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<WardrobeOutfit>> GetAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT slot, figure, gender FROM users_wardrobe WHERE user_ref = @userId ORDER BY slot;", connection);
        command.Parameters.AddWithValue("@userId", userId);

        var outfits = new List<WardrobeOutfit>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            outfits.Add(new WardrobeOutfit(reader.GetByte(0), reader.GetString(1), reader.GetString(2)[0]));
        }

        return outfits;
    }

    public async Task SaveAsync(int userId, WardrobeOutfit outfit, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            INSERT INTO users_wardrobe (user_ref, slot, figure, gender) VALUES (@userId, @slot, @figure, @gender)
            ON DUPLICATE KEY UPDATE figure = VALUES(figure), gender = VALUES(gender);
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@slot", outfit.Slot);
        command.Parameters.AddWithValue("@figure", outfit.Figure);
        command.Parameters.AddWithValue("@gender", outfit.Gender.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
