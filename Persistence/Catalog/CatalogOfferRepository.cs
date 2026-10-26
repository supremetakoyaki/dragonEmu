using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class CatalogOfferRepository : ICatalogOfferRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CatalogOfferRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<CatalogOffer>> GetByPageIdAsync(int pageId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT o.id, o.ref_page, o.offer_code, o.price_credits, o.price_activity_points, o.club_only,
                   o.giftable, p.id, p.product_type, p.product_extra_data, p.product_count,
                   p.product_rental_duration, fd.ref_furnidata, p.ref_furni_def
            FROM definition_catalog_page_offers o
            LEFT JOIN definition_catalog_page_offers_products p ON p.ref_offer_id = o.id
            LEFT JOIN definition_furniture fd ON fd.id = p.ref_furni_def
            WHERE o.ref_page = @pageId
            ORDER BY o.id, p.id;
            """,
            connection);
        command.Parameters.AddWithValue("@pageId", pageId);

        var offers = new List<CatalogOffer>();
        int? currentOfferId = null;
        List<CatalogOfferProduct> currentProducts = [];

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            int offerId = reader.GetInt32(0);
            if (offerId != currentOfferId)
            {
                currentOfferId = offerId;
                currentProducts = [];
                offers.Add(new CatalogOffer(
                    offerId,
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.GetInt32(3),
                    reader.GetInt32(4),
                    reader.GetBoolean(5),
                    reader.GetBoolean(6),
                    currentProducts));
            }

            if (!reader.IsDBNull(7))
            {
                int? furnidataId = reader.IsDBNull(12) ? null : reader.GetInt32(12);
                int? furnitureDefId = reader.IsDBNull(13) ? null : reader.GetInt32(13);
                CatalogProductType productType = CatalogProductTypeExtensions.FromWireChar(reader.GetString(8)[0]);
                string? extraData = reader.IsDBNull(9) ? null : reader.GetString(9);

                int wireId = productType == CatalogProductType.Effect && int.TryParse(extraData, out int effectId)
                    ? effectId
                    : furnidataId ?? 0;

                currentProducts.Add(new CatalogOfferProduct(
                    ProductType: productType,
                    WireId: wireId,
                    ExtraData: extraData,
                    Quantity: reader.GetInt32(10),
                    RentalDuration: reader.GetInt32(11),
                    FurnitureDefId: furnitureDefId));
            }
        }

        return offers;
    }
}
