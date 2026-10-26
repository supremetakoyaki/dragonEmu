using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class ClubGiftRepository : IClubGiftRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ClubGiftRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ClubGiftDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT g.id, g.offer_code, g.months_required,
                   g.product_type, g.product_count,
                   fd.ref_furnidata, g.ref_furni_def
            FROM definition_catalog_club_gifts g
            LEFT JOIN definition_furniture fd ON fd.id = g.ref_furni_def
            ORDER BY g.months_required, g.id;
            """,
            connection);

        var gifts = new List<ClubGiftDefinition>();

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            int? furnidataId = reader.IsDBNull(5) ? null : reader.GetInt32(5);
            int? furnitureDefId = reader.IsDBNull(6) ? null : reader.GetInt32(6);

            var product = new CatalogOfferProduct(
                ProductType: CatalogProductTypeExtensions.FromWireChar(reader.GetString(3)[0]),
                WireId: furnidataId ?? 0,
                ExtraData: null,
                Quantity: reader.GetInt32(4),
                RentalDuration: -1,
                FurnitureDefId: furnitureDefId);

            gifts.Add(new ClubGiftDefinition(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetInt32(2),
                product));
        }

        return gifts;
    }
}
