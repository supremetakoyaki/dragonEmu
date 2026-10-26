using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class MarketplaceRepository : IMarketplaceRepository
{
    private const string Live = "o.state = 'open' AND o.expires_at > UTC_TIMESTAMP()";

    private readonly IDbConnectionFactory _connectionFactory;

    public MarketplaceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<MarketplaceConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT commission_percent, token_batch_price, token_batch_size, offer_min_price, offer_max_price,
                   offer_expiration_hours, average_price_period_days, can_sell_whats_in_catalog
            FROM definition_catalog_marketplace_config
            LIMIT 1;
            """, connection);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return MarketplaceConfig.Default;
        }

        return new MarketplaceConfig(
            CommissionPercent: Convert.ToInt32(reader.GetValue(0)),
            TokenBatchPrice: Convert.ToInt32(reader.GetValue(1)),
            TokenBatchSize: Convert.ToInt32(reader.GetValue(2)),
            OfferMinPrice: Convert.ToInt32(reader.GetValue(3)),
            OfferMaxPrice: Convert.ToInt32(reader.GetValue(4)),
            OfferExpirationHours: Convert.ToInt32(reader.GetValue(5)),
            AveragePricePeriodDays: Convert.ToInt32(reader.GetValue(6)),
            CanSellWhatsInCatalog: reader.GetString(7) == "1");
    }

    public async Task<int> GetTokensAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT marketplace_tokens FROM users WHERE id = @userId;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        return await command.ExecuteScalarAsync(cancellationToken) is { } value and not DBNull ? Convert.ToInt32(value) : 0;
    }

    public async Task<(long Credits, int Tokens)?> BuyTokensAsync(int userId, int price, int batchSize, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        if (price > 0 && !await TakeCreditsAsync(connection, transaction, userId, price, cancellationToken))
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        await using (var add = new MySqlCommand(
            "UPDATE users SET marketplace_tokens = marketplace_tokens + @batch WHERE id = @userId;", connection, transaction))
        {
            add.Parameters.AddWithValue("@batch", batchSize);
            add.Parameters.AddWithValue("@userId", userId);
            await add.ExecuteNonQueryAsync(cancellationToken);
        }

        long credits = await CreditsAsync(connection, transaction, userId, cancellationToken);
        int tokens;
        await using (var read = new MySqlCommand("SELECT marketplace_tokens FROM users WHERE id = @userId;", connection, transaction))
        {
            read.Parameters.AddWithValue("@userId", userId);
            tokens = Convert.ToInt32(await read.ExecuteScalarAsync(cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
        return (credits, tokens);
    }

    public async Task<MarketplaceOfferCreation> CreateOfferAsync(
        int userId, int itemId, int askingPrice, int price, DateTime expiresAt, bool refuseCatalogueFurniture, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        int? furniDefId;
        await using (var item = new MySqlCommand(
            """
            SELECT i.ref_furni_def
            FROM users_items i
            INNER JOIN definition_furniture fd ON fd.id = i.ref_furni_def
            WHERE i.id = @itemId AND i.user_ref = @userId AND i.room_ref IS NULL AND i.expires_at IS NULL AND fd.allow_trade = 1
              AND NOT EXISTS (SELECT 1 FROM marketplace_offers m WHERE m.item_ref = i.id)
            FOR UPDATE;
            """, connection, transaction))
        {
            item.Parameters.AddWithValue("@itemId", itemId);
            item.Parameters.AddWithValue("@userId", userId);
            furniDefId = await item.ExecuteScalarAsync(cancellationToken) is { } value and not DBNull ? Convert.ToInt32(value) : null;
        }

        if (furniDefId is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return MarketplaceOfferCreation.NotSellable;
        }

        if (refuseCatalogueFurniture)
        {
            await using var catalogue = new MySqlCommand(
                """
                SELECT 1
                FROM definition_catalog_page_offers_products p
                INNER JOIN definition_catalog_page_offers o ON o.id = p.ref_offer_id
                INNER JOIN definition_catalog_page_header h ON h.id = o.ref_page
                WHERE p.ref_furni_def = @furniDefId AND p.product_rental_duration <= 0 AND h.enabled = '1' AND h.min_rank < 2
                LIMIT 1;
                """, connection, transaction);
            catalogue.Parameters.AddWithValue("@furniDefId", furniDefId);
            if (await catalogue.ExecuteScalarAsync(cancellationToken) is not null and not DBNull)
            {
                await transaction.RollbackAsync(cancellationToken);
                return MarketplaceOfferCreation.InCatalogue;
            }
        }

        await using (var token = new MySqlCommand(
            "UPDATE users SET marketplace_tokens = marketplace_tokens - 1 WHERE id = @userId AND marketplace_tokens > 0;", connection, transaction))
        {
            token.Parameters.AddWithValue("@userId", userId);
            if (await token.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return MarketplaceOfferCreation.NoTokens;
            }
        }

        await using (var insert = new MySqlCommand(
            """
            INSERT INTO marketplace_offers (item_ref, seller_ref, furni_def_ref, asking_price, price, expires_at)
            VALUES (@itemId, @userId, @furniDefId, @askingPrice, @price, @expiresAt);
            """, connection, transaction))
        {
            insert.Parameters.AddWithValue("@itemId", itemId);
            insert.Parameters.AddWithValue("@userId", userId);
            insert.Parameters.AddWithValue("@furniDefId", furniDefId);
            insert.Parameters.AddWithValue("@askingPrice", askingPrice);
            insert.Parameters.AddWithValue("@price", price);
            insert.Parameters.AddWithValue("@expiresAt", expiresAt);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return MarketplaceOfferCreation.Created;
    }

    public async Task<IReadOnlyList<MarketplaceListing>> SearchAsync(
        int viewerId, int minPrice, int maxPrice, string name, MarketplaceSort sort, int averageDays, DateTime todayStartUtc, int limit,
        CancellationToken cancellationToken = default)
    {
        string order = sort switch
        {
            MarketplaceSort.MostExpensive => "l.price DESC",
            MarketplaceSort.Cheapest => "l.price ASC",
            MarketplaceSort.MostTradedToday => "sold_today DESC",
            MarketplaceSort.LeastTradedToday => "sold_today ASC",
            MarketplaceSort.MostOffers => "l.offers DESC",
            _ => "l.offers ASC",
        };

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            $"""
            WITH live AS (
                SELECT o.id, o.furni_def_ref, o.price, o.expires_at,
                       ROW_NUMBER() OVER (PARTITION BY o.furni_def_ref ORDER BY o.price, o.id) AS place,
                       COUNT(*) OVER (PARTITION BY o.furni_def_ref) AS offers
                FROM marketplace_offers o
                WHERE {Live} AND o.seller_ref <> @viewerId
            )
            SELECT l.id, fd.type, fd.ref_furnidata, l.price, TIMESTAMPDIFF(MINUTE, UTC_TIMESTAMP(), l.expires_at), l.offers,
                   (SELECT COALESCE(ROUND(AVG(s.price)), 0) FROM historical_marketplace_sales s
                    WHERE s.furni_def_ref = l.furni_def_ref AND s.sold_at >= UTC_TIMESTAMP() - INTERVAL @averageDays DAY) AS average,
                   (SELECT COUNT(*) FROM historical_marketplace_sales s
                    WHERE s.furni_def_ref = l.furni_def_ref AND s.sold_at >= @todayStart) AS sold_today
            FROM live l
            INNER JOIN definition_furniture fd ON fd.id = l.furni_def_ref
            WHERE l.place = 1
              AND (@minPrice < 0 OR l.price >= @minPrice) AND (@maxPrice < 0 OR l.price <= @maxPrice)
              AND (@name = '' OR fd.localization_name LIKE @pattern)
            ORDER BY {order}, fd.id
            LIMIT @limit;
            """, connection);
        command.Parameters.AddWithValue("@viewerId", viewerId);
        command.Parameters.AddWithValue("@minPrice", minPrice);
        command.Parameters.AddWithValue("@maxPrice", maxPrice);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@pattern", $"%{EscapeLike(name)}%");
        command.Parameters.AddWithValue("@averageDays", averageDays);
        command.Parameters.AddWithValue("@todayStart", todayStartUtc);
        command.Parameters.AddWithValue("@limit", limit);

        var listings = new List<MarketplaceListing>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            listings.Add(new MarketplaceListing(
                OfferId: reader.GetInt32(0),
                FurniType: FurniTypeOf(reader.GetString(1)),
                Sprite: reader.GetInt32(2),
                Price: reader.GetInt32(3),
                MinutesLeft: Math.Max(0, Convert.ToInt32(reader.GetValue(4))),
                AveragePrice: Convert.ToInt32(reader.GetValue(6)),
                OfferCount: Convert.ToInt32(reader.GetValue(5))));
        }

        return listings;
    }

    public async Task<MarketplaceOwnOffers> GetOwnOffersAsync(int sellerId, int averageDays, int limit, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        int creditsWaiting;
        await using (var waiting = new MySqlCommand(
            "SELECT COALESCE(SUM(asking_price), 0) FROM marketplace_offers WHERE seller_ref = @sellerId AND state = 'sold';", connection))
        {
            waiting.Parameters.AddWithValue("@sellerId", sellerId);
            creditsWaiting = Convert.ToInt32(await waiting.ExecuteScalarAsync(cancellationToken));
        }

        await using var command = new MySqlCommand(
            """
            SELECT o.id, o.state, o.expires_at, fd.type, fd.ref_furnidata, o.asking_price,
                   (SELECT COALESCE(ROUND(AVG(s.price)), 0) FROM historical_marketplace_sales s
                    WHERE s.furni_def_ref = o.furni_def_ref AND s.sold_at >= UTC_TIMESTAMP() - INTERVAL @averageDays DAY)
            FROM marketplace_offers o
            INNER JOIN definition_furniture fd ON fd.id = o.furni_def_ref
            WHERE o.seller_ref = @sellerId
            ORDER BY o.id DESC
            LIMIT @limit;
            """, connection);
        command.Parameters.AddWithValue("@sellerId", sellerId);
        command.Parameters.AddWithValue("@averageDays", averageDays);
        command.Parameters.AddWithValue("@limit", limit);

        DateTime now = DateTime.UtcNow;
        var offers = new List<MarketplaceOwnOffer>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            DateTime expiresAt = Utc(reader.GetDateTime(2));
            MarketplaceOfferStatus status = reader.GetString(1) == "sold" ? MarketplaceOfferStatus.Sold
                : expiresAt <= now ? MarketplaceOfferStatus.Expired
                : MarketplaceOfferStatus.Open;
            offers.Add(new MarketplaceOwnOffer(
                OfferId: reader.GetInt32(0),
                Status: status,
                FurniType: FurniTypeOf(reader.GetString(3)),
                Sprite: reader.GetInt32(4),
                Price: reader.GetInt32(5),
                MinutesLeft: status == MarketplaceOfferStatus.Open ? (int)Math.Ceiling((expiresAt - now).TotalMinutes) : 0,
                AveragePrice: Convert.ToInt32(reader.GetValue(6))));
        }

        return new MarketplaceOwnOffers(creditsWaiting, offers);
    }

    public async Task<(int AveragePrice, int OfferCount, IReadOnlyList<MarketplaceSale> Sales)> GetItemStatsAsync(
        int viewerId, int furniType, int sprite, int averageDays, DateTime since, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            $"""
            SELECT
                (SELECT COALESCE(ROUND(AVG(s.price)), 0) FROM historical_marketplace_sales s
                 INNER JOIN definition_furniture fd ON fd.id = s.furni_def_ref
                 WHERE fd.type = @type AND fd.ref_furnidata = @sprite AND s.sold_at >= UTC_TIMESTAMP() - INTERVAL @averageDays DAY),
                (SELECT COUNT(*) FROM marketplace_offers o
                 INNER JOIN definition_furniture fd ON fd.id = o.furni_def_ref
                 WHERE fd.type = @type AND fd.ref_furnidata = @sprite AND {Live} AND o.seller_ref <> @viewerId);
            SELECT s.sold_at, s.price
            FROM historical_marketplace_sales s
            INNER JOIN definition_furniture fd ON fd.id = s.furni_def_ref
            WHERE fd.type = @type AND fd.ref_furnidata = @sprite AND s.sold_at >= @since
            ORDER BY s.sold_at;
            """, connection);
        command.Parameters.AddWithValue("@type", furniType == MarketplaceFurniType.Floor ? "s" : "i");
        command.Parameters.AddWithValue("@sprite", sprite);
        command.Parameters.AddWithValue("@averageDays", averageDays);
        command.Parameters.AddWithValue("@viewerId", viewerId);
        command.Parameters.AddWithValue("@since", since);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        int average = Convert.ToInt32(reader.GetValue(0));
        int offers = Convert.ToInt32(reader.GetValue(1));

        var sales = new List<MarketplaceSale>();
        await reader.NextResultAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            sales.Add(new MarketplaceSale(Utc(reader.GetDateTime(0)), reader.GetInt32(1)));
        }

        return (average, offers, sales);
    }

    public async Task<MarketplacePurchase> BuyAsync(int buyerId, int requestedOfferId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        int furniDefId;
        int requestedPrice;
        await using (var requested = new MySqlCommand(
            "SELECT furni_def_ref, price FROM marketplace_offers WHERE id = @offerId;", connection, transaction))
        {
            requested.Parameters.AddWithValue("@offerId", requestedOfferId);
            await using MySqlDataReader reader = await requested.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return new MarketplacePurchase(MarketplaceBuyResult.SoldOut, 0, 0);
            }

            furniDefId = reader.GetInt32(0);
            requestedPrice = reader.GetInt32(1);
        }

        int offerId, price, itemId, sellerId, askingPrice;
        await using (var cheapest = new MySqlCommand(
            $"""
            SELECT o.id, o.price, o.item_ref, o.seller_ref, o.asking_price
            FROM marketplace_offers o
            WHERE o.furni_def_ref = @furniDefId AND {Live} AND o.seller_ref <> @buyerId
            ORDER BY o.price, o.id
            LIMIT 1
            FOR UPDATE;
            """, connection, transaction))
        {
            cheapest.Parameters.AddWithValue("@furniDefId", furniDefId);
            cheapest.Parameters.AddWithValue("@buyerId", buyerId);
            await using MySqlDataReader reader = await cheapest.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return new MarketplacePurchase(MarketplaceBuyResult.SoldOut, 0, 0);
            }

            (offerId, price, itemId, sellerId, askingPrice) = (reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4));
        }

        if (price > requestedPrice)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new MarketplacePurchase(MarketplaceBuyResult.NewPrice, offerId, price);
        }

        if (!await TakeCreditsAsync(connection, transaction, buyerId, price, cancellationToken))
        {
            await transaction.RollbackAsync(cancellationToken);
            return new MarketplacePurchase(MarketplaceBuyResult.NotEnoughCredits, offerId, price);
        }

        await using (var sold = new MySqlCommand(
            "UPDATE marketplace_offers SET state = 'sold', sold_at = UTC_TIMESTAMP(), buyer_ref = @buyerId, item_ref = NULL WHERE id = @offerId;",
            connection, transaction))
        {
            sold.Parameters.AddWithValue("@buyerId", buyerId);
            sold.Parameters.AddWithValue("@offerId", offerId);
            await sold.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var item = new MySqlCommand(
            "UPDATE users_items SET user_ref = @buyerId WHERE id = @itemId AND room_ref IS NULL;", connection, transaction))
        {
            item.Parameters.AddWithValue("@buyerId", buyerId);
            item.Parameters.AddWithValue("@itemId", itemId);
            if (await item.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new MarketplacePurchase(MarketplaceBuyResult.SoldOut, 0, 0);
            }
        }

        await using (var history = new MySqlCommand(
            """
            INSERT INTO historical_marketplace_sales (sold_at, offer_ref, item_ref, furni_def_ref, seller_ref, buyer_ref, asking_price, price)
            VALUES (UTC_TIMESTAMP(), @offerId, @itemId, @furniDefId, @sellerId, @buyerId, @askingPrice, @price);
            """, connection, transaction))
        {
            history.Parameters.AddWithValue("@offerId", offerId);
            history.Parameters.AddWithValue("@itemId", itemId);
            history.Parameters.AddWithValue("@furniDefId", furniDefId);
            history.Parameters.AddWithValue("@sellerId", sellerId);
            history.Parameters.AddWithValue("@buyerId", buyerId);
            history.Parameters.AddWithValue("@askingPrice", askingPrice);
            history.Parameters.AddWithValue("@price", price);
            await history.ExecuteNonQueryAsync(cancellationToken);
        }

        long creditsLeft = await CreditsAsync(connection, transaction, buyerId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new MarketplacePurchase(MarketplaceBuyResult.Bought, offerId, price, itemId, creditsLeft);
    }

    public async Task<int?> CancelAsync(int sellerId, int offerId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        int? itemId;
        await using (var offer = new MySqlCommand(
            "SELECT item_ref FROM marketplace_offers WHERE id = @offerId AND seller_ref = @sellerId AND state = 'open' FOR UPDATE;",
            connection, transaction))
        {
            offer.Parameters.AddWithValue("@offerId", offerId);
            offer.Parameters.AddWithValue("@sellerId", sellerId);
            itemId = await offer.ExecuteScalarAsync(cancellationToken) is { } value and not DBNull ? Convert.ToInt32(value) : null;
        }

        if (itemId is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        await using (var delete = new MySqlCommand("DELETE FROM marketplace_offers WHERE id = @offerId;", connection, transaction))
        {
            delete.Parameters.AddWithValue("@offerId", offerId);
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return itemId;
    }

    public async Task<(int Credits, long Balance)> RedeemAsync(int sellerId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        var soldIds = new List<int>();
        int credits = 0;
        await using (var sold = new MySqlCommand(
            "SELECT id, asking_price FROM marketplace_offers WHERE seller_ref = @sellerId AND state = 'sold' FOR UPDATE;", connection, transaction))
        {
            sold.Parameters.AddWithValue("@sellerId", sellerId);
            await using MySqlDataReader reader = await sold.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                soldIds.Add(reader.GetInt32(0));
                credits += reader.GetInt32(1);
            }
        }

        if (soldIds.Count == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return (0, 0);
        }

        await using (var delete = new MySqlCommand(
            $"DELETE FROM marketplace_offers WHERE id IN ({string.Join(", ", soldIds)});", connection, transaction))
        {
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var pay = new MySqlCommand(
            """
            INSERT INTO users_currency (user_ref, currency, value) VALUES (@sellerId, @currency, @credits)
            ON DUPLICATE KEY UPDATE value = value + VALUES(value);
            """, connection, transaction))
        {
            pay.Parameters.AddWithValue("@sellerId", sellerId);
            pay.Parameters.AddWithValue("@currency", CurrencyType.Credits);
            pay.Parameters.AddWithValue("@credits", credits);
            await pay.ExecuteNonQueryAsync(cancellationToken);
        }

        long balance = await CreditsAsync(connection, transaction, sellerId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return (credits, balance);
    }

    private static async Task<bool> TakeCreditsAsync(
        MySqlConnection connection, MySqlTransaction transaction, int userId, int amount, CancellationToken cancellationToken)
    {
        await using var take = new MySqlCommand(
            "UPDATE users_currency SET value = value - @amount WHERE user_ref = @userId AND currency = @currency AND value >= @amount;",
            connection, transaction);
        take.Parameters.AddWithValue("@amount", amount);
        take.Parameters.AddWithValue("@userId", userId);
        take.Parameters.AddWithValue("@currency", CurrencyType.Credits);
        return await take.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    private static async Task<long> CreditsAsync(MySqlConnection connection, MySqlTransaction transaction, int userId, CancellationToken cancellationToken)
    {
        await using var read = new MySqlCommand(
            "SELECT value FROM users_currency WHERE user_ref = @userId AND currency = @currency;", connection, transaction);
        read.Parameters.AddWithValue("@userId", userId);
        read.Parameters.AddWithValue("@currency", CurrencyType.Credits);
        return await read.ExecuteScalarAsync(cancellationToken) is { } value and not DBNull ? Convert.ToInt64(value) : 0;
    }

    private static int FurniTypeOf(string type) => type == "s" ? MarketplaceFurniType.Floor : MarketplaceFurniType.Wall;

    private static DateTime Utc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static string EscapeLike(string text) => text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
