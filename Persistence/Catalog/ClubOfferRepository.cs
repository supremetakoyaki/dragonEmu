using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class ClubOfferRepository : IClubOfferRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ClubOfferRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ClubOfferDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT ref_offer_id, period, vip = '1', upgrade = '1' FROM definition_catalog_club_offers ORDER BY period, vip, ref_offer_id;",
            connection);

        var offers = new List<ClubOfferDefinition>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            offers.Add(new ClubOfferDefinition(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2) == 1, reader.GetInt32(3) == 1));
        }

        return offers;
    }
}
