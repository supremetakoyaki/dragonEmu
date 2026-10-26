using System.Globalization;
using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class UserItemRepository : IUserItemRepository
{
    private const int MaxPositionsPerStatement = 1000;

    private const string BaseSelect =
        """
        SELECT i.id, i.user_ref, fd.id, fd.ref_furnidata, fd.classname, fd.type, fd.logic_class,
               fd.allow_trade, fd.allow_recycle, i.extra_data, i.room_ref, i.x, i.y, i.z, i.rotation,
               i.wall_position, i.seen, i.extras, i.expires_at,
               EXISTS (SELECT 1 FROM marketplace_offers m WHERE m.item_ref = i.id)
        FROM users_items i
        INNER JOIN definition_furniture fd ON fd.id = i.ref_furni_def
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public UserItemRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateInInventoryAsync(int userId, int furniDefId, string? extraData, CancellationToken cancellationToken = default) =>
        await CreateInInventoryAsync(userId, furniDefId, extraData, seen: true, extras: null, cancellationToken);

    public async Task<int> CreateInInventoryAsync(int userId, int furniDefId, string? extraData, bool seen, CancellationToken cancellationToken = default) =>
        await CreateInInventoryAsync(userId, furniDefId, extraData, seen, extras: null, cancellationToken);

    public async Task<int> CreateInInventoryAsync(
        int userId, int furniDefId, string? extraData, bool seen, int? extras, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "INSERT INTO users_items (user_ref, ref_furni_def, extra_data, seen, extras) VALUES (@userId, @furniDefId, @extraData, @seen, @extras);",
            connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@furniDefId", furniDefId);
        command.Parameters.AddWithValue("@extraData", (object?)extraData ?? DBNull.Value);
        command.Parameters.AddWithValue("@seen", seen);
        command.Parameters.AddWithValue("@extras", (object?)extras ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return (int)command.LastInsertedId;
    }

    public async Task<UserItem?> GetByIdAsync(int itemId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand($"{BaseSelect} WHERE i.id = @itemId;", connection);
        command.Parameters.AddWithValue("@itemId", itemId);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapRow(reader) : null;
    }

    public async Task<IReadOnlyList<UserItem>> GetByIdsAsync(IReadOnlyList<int> itemIds, CancellationToken cancellationToken = default)
    {
        if (itemIds.Count == 0)
        {
            return [];
        }

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            $"{BaseSelect} WHERE i.id IN ({string.Join(", ", itemIds.Select((_, i) => $"@id{i}"))});", connection);
        for (int i = 0; i < itemIds.Count; i++)
        {
            command.Parameters.AddWithValue($"@id{i}", itemIds[i]);
        }

        var found = new Dictionary<int, UserItem>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            UserItem item = MapRow(reader);
            found[item.Id] = item;
        }

        return [.. itemIds.Where(found.ContainsKey).Select(id => found[id])];
    }

    public async Task<bool> UpdateExtraDataAsync(int itemId, string? extraData, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("UPDATE users_items SET extra_data = @extraData WHERE id = @itemId;", connection);
        command.Parameters.AddWithValue("@itemId", itemId);
        command.Parameters.AddWithValue("@extraData", (object?)extraData ?? DBNull.Value);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IReadOnlyDictionary<int, string?>> GetExtraDataInRoomAsync(
        int roomId, IReadOnlyCollection<int> itemIds, CancellationToken cancellationToken = default)
    {
        var extraData = new Dictionary<int, string?>();
        if (itemIds.Count == 0)
        {
            return extraData;
        }

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            $"SELECT id, extra_data FROM users_items WHERE room_ref = @roomId AND id IN ({string.Join(", ", itemIds.Select((_, i) => $"@id{i}"))});",
            connection);
        command.Parameters.AddWithValue("@roomId", roomId);
        int index = 0;
        foreach (int itemId in itemIds)
        {
            command.Parameters.AddWithValue($"@id{index++}", itemId);
        }

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            extraData[reader.GetInt32(0)] = reader.IsDBNull(1) ? null : reader.GetString(1);
        }

        return extraData;
    }

    public async Task<IReadOnlyList<UserItem>> GetInventoryAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand($"{BaseSelect} WHERE i.user_ref = @userId AND i.room_ref IS NULL AND NOT EXISTS (SELECT 1 FROM marketplace_offers m WHERE m.item_ref = i.id);", connection);
        command.Parameters.AddWithValue("@userId", userId);

        return await ReadAllAsync(command, cancellationToken);
    }

    public async Task<IReadOnlyList<UserItem>> GetFloorItemsInRoomAsync(int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            $"{BaseSelect} WHERE i.room_ref = @roomId AND i.wall_position IS NULL;", connection);
        command.Parameters.AddWithValue("@roomId", roomId);

        return await ReadAllAsync(command, cancellationToken);
    }

    public async Task<IReadOnlyList<UserItem>> GetWallItemsInRoomAsync(int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            $"{BaseSelect} WHERE i.room_ref = @roomId AND i.wall_position IS NOT NULL;", connection);
        command.Parameters.AddWithValue("@roomId", roomId);

        return await ReadAllAsync(command, cancellationToken);
    }

    public async Task<bool> PlaceOnFloorAsync(
        int itemId, int userId, int roomId, int x, int y, decimal z, int rotation, string? basalExtraData, bool resetExtraData,
        CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            UPDATE users_items
            SET room_ref = @roomId, x = @x, y = @y, z = @z, rotation = @rotation, seen = TRUE,
                extra_data = IF(@basalExtraData IS NOT NULL AND (@resetExtraData OR extra_data IS NULL OR extra_data = ''), @basalExtraData, extra_data)
            WHERE id = @itemId AND user_ref = @userId AND room_ref IS NULL AND NOT EXISTS (SELECT 1 FROM marketplace_offers m WHERE m.item_ref = users_items.id);
            """,
            connection);
        command.Parameters.AddWithValue("@itemId", itemId);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@x", x);
        command.Parameters.AddWithValue("@y", y);
        command.Parameters.AddWithValue("@z", z);
        command.Parameters.AddWithValue("@rotation", rotation);
        command.Parameters.AddWithValue("@basalExtraData", (object?)basalExtraData ?? DBNull.Value);
        command.Parameters.AddWithValue("@resetExtraData", resetExtraData);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> PlaceOnWallAsync(
        int itemId, int userId, int roomId, string wallPosition, string? basalExtraData, bool resetExtraData,
        CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            UPDATE users_items
            SET room_ref = @roomId, wall_position = @wallPosition, seen = TRUE,
                extra_data = IF(@basalExtraData IS NOT NULL AND (@resetExtraData OR extra_data IS NULL OR extra_data = ''), @basalExtraData, extra_data)
            WHERE id = @itemId AND user_ref = @userId AND room_ref IS NULL AND NOT EXISTS (SELECT 1 FROM marketplace_offers m WHERE m.item_ref = users_items.id);
            """,
            connection);
        command.Parameters.AddWithValue("@itemId", itemId);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@wallPosition", wallPosition);
        command.Parameters.AddWithValue("@basalExtraData", (object?)basalExtraData ?? DBNull.Value);
        command.Parameters.AddWithValue("@resetExtraData", resetExtraData);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> MoveOnWallAsync(int itemId, int roomId, string wallPosition, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "UPDATE users_items SET wall_position = @wallPosition WHERE id = @itemId AND room_ref = @roomId;", connection);
        command.Parameters.AddWithValue("@wallPosition", wallPosition);
        command.Parameters.AddWithValue("@itemId", itemId);
        command.Parameters.AddWithValue("@roomId", roomId);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> MoveOnFloorAsync(
        int itemId, int roomId, int x, int y, decimal z, int rotation, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            UPDATE users_items
            SET x = @x, y = @y, z = @z, rotation = @rotation
            WHERE id = @itemId AND room_ref = @roomId;
            """,
            connection);
        command.Parameters.AddWithValue("@itemId", itemId);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@x", x);
        command.Parameters.AddWithValue("@y", y);
        command.Parameters.AddWithValue("@z", z);
        command.Parameters.AddWithValue("@rotation", rotation);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task SaveFloorPositionsAsync(int roomId, IReadOnlyList<FloorItemPosition> positions, CancellationToken cancellationToken = default)
    {
        if (positions.Count == 0)
        {
            return;
        }

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        foreach (FloorItemPosition[] chunk in positions.Chunk(MaxPositionsPerStatement))
        {
            await using var command = new MySqlCommand { Connection = connection };
            var rows = new List<string>(chunk.Length);
            for (int i = 0; i < chunk.Length; i++)
            {
                rows.Add($"ROW(@id{i}, @x{i}, @y{i}, @z{i})");
                command.Parameters.AddWithValue($"@id{i}", chunk[i].ItemId);
                command.Parameters.AddWithValue($"@x{i}", chunk[i].X);
                command.Parameters.AddWithValue($"@y{i}", chunk[i].Y);
                command.Parameters.AddWithValue($"@z{i}", Math.Round((decimal)chunk[i].Z, 4));
            }

            command.CommandText =
                $"""
                UPDATE users_items i
                JOIN (VALUES {string.Join(", ", rows)}) AS p (id, x, y, z) ON p.id = i.id
                SET i.x = p.x, i.y = p.y, i.z = p.z
                WHERE i.room_ref = @roomId;
                """;
            command.Parameters.AddWithValue("@roomId", roomId);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<int>> PickAllAsync(int roomId, int newOwnerId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        var itemIds = new List<int>();
        await using (var select = new MySqlCommand(
            """
            SELECT i.id
            FROM users_items i
            JOIN definition_furniture d ON d.id = i.ref_furni_def
            WHERE i.room_ref = @roomId AND d.logic_class <> 'stickie'
            FOR UPDATE OF i;
            """, connection, transaction))
        {
            select.Parameters.AddWithValue("@roomId", roomId);
            await using MySqlDataReader reader = await select.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                itemIds.Add(reader.GetInt32(0));
            }
        }

        await using (var pick = new MySqlCommand(
            """
            UPDATE users_items i
            JOIN definition_furniture d ON d.id = i.ref_furni_def
            SET i.user_ref = @newOwnerId, i.room_ref = NULL, i.x = NULL, i.y = NULL, i.z = NULL, i.rotation = NULL, i.wall_position = NULL
            WHERE i.room_ref = @roomId AND d.logic_class <> 'stickie';
            """, connection, transaction))
        {
            pick.Parameters.AddWithValue("@roomId", roomId);
            pick.Parameters.AddWithValue("@newOwnerId", newOwnerId);
            await pick.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return itemIds;
    }

    public async Task<bool> PickupFromFloorAsync(int itemId, int roomId, int pickerUserId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            UPDATE users_items
            SET user_ref = @pickerUserId, room_ref = NULL, x = NULL, y = NULL, z = NULL, rotation = NULL
            WHERE id = @itemId AND room_ref = @roomId;
            """,
            connection);
        command.Parameters.AddWithValue("@itemId", itemId);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@pickerUserId", pickerUserId);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> PickupFromWallAsync(int itemId, int roomId, int pickerUserId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            UPDATE users_items
            SET user_ref = @pickerUserId, room_ref = NULL, wall_position = NULL
            WHERE id = @itemId AND room_ref = @roomId AND wall_position IS NOT NULL;
            """,
            connection);
        command.Parameters.AddWithValue("@itemId", itemId);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@pickerUserId", pickerUserId);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteFromInventoryAsync(int itemId, int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "DELETE FROM users_items WHERE id = @itemId AND user_ref = @userId AND room_ref IS NULL AND NOT EXISTS (SELECT 1 FROM marketplace_offers m WHERE m.item_ref = users_items.id);",
            connection);
        command.Parameters.AddWithValue("@itemId", itemId);
        command.Parameters.AddWithValue("@userId", userId);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<StickiePlacement?> PlaceStickieAsync(
        int padId, int userId, int roomId, string wallPosition, string noteData, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        int furniDefId;
        int notes;
        await using (var select = new MySqlCommand(
            "SELECT ref_furni_def, extra_data FROM users_items WHERE id = @padId AND user_ref = @userId AND room_ref IS NULL AND NOT EXISTS (SELECT 1 FROM marketplace_offers m WHERE m.item_ref = users_items.id) FOR UPDATE;",
            connection, transaction))
        {
            select.Parameters.AddWithValue("@padId", padId);
            select.Parameters.AddWithValue("@userId", userId);
            await using MySqlDataReader reader = await select.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            furniDefId = reader.GetInt32(0);
            notes = Stickie.NotesIn(reader.IsDBNull(1) ? null : reader.GetString(1));
        }

        int noteId;
        if (notes > 1)
        {
            await using (var takeOne = new MySqlCommand("UPDATE users_items SET extra_data = @notesLeft WHERE id = @padId;", connection, transaction))
            {
                takeOne.Parameters.AddWithValue("@notesLeft", (notes - 1).ToString(CultureInfo.InvariantCulture));
                takeOne.Parameters.AddWithValue("@padId", padId);
                await takeOne.ExecuteNonQueryAsync(cancellationToken);
            }

            await using var create = new MySqlCommand(
                """
                INSERT INTO users_items (user_ref, ref_furni_def, extra_data, room_ref, wall_position, seen)
                VALUES (@userId, @furniDefId, @noteData, @roomId, @wallPosition, TRUE);
                """, connection, transaction);
            create.Parameters.AddWithValue("@userId", userId);
            create.Parameters.AddWithValue("@furniDefId", furniDefId);
            create.Parameters.AddWithValue("@noteData", noteData);
            create.Parameters.AddWithValue("@roomId", roomId);
            create.Parameters.AddWithValue("@wallPosition", wallPosition);
            await create.ExecuteNonQueryAsync(cancellationToken);
            noteId = (int)create.LastInsertedId;
        }
        else
        {
            await using var placeLast = new MySqlCommand(
                "UPDATE users_items SET room_ref = @roomId, wall_position = @wallPosition, extra_data = @noteData, seen = TRUE WHERE id = @padId;",
                connection, transaction);
            placeLast.Parameters.AddWithValue("@roomId", roomId);
            placeLast.Parameters.AddWithValue("@wallPosition", wallPosition);
            placeLast.Parameters.AddWithValue("@noteData", noteData);
            placeLast.Parameters.AddWithValue("@padId", padId);
            await placeLast.ExecuteNonQueryAsync(cancellationToken);
            noteId = padId;
        }

        await transaction.CommitAsync(cancellationToken);
        return new StickiePlacement(noteId, notes - 1);
    }

    public async Task<bool> DeleteFromRoomAsync(int itemId, int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("DELETE FROM users_items WHERE id = @itemId AND room_ref = @roomId;", connection);
        command.Parameters.AddWithValue("@itemId", itemId);
        command.Parameters.AddWithValue("@roomId", roomId);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(int itemId, int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "DELETE FROM users_items WHERE id = @itemId AND user_ref = @userId;",
            connection);
        command.Parameters.AddWithValue("@itemId", itemId);
        command.Parameters.AddWithValue("@userId", userId);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private static async Task<IReadOnlyList<UserItem>> ReadAllAsync(MySqlCommand command, CancellationToken cancellationToken)
    {
        var results = new List<UserItem>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(MapRow(reader));
        }

        return results;
    }

    public async Task SetRentalExpiryAsync(IReadOnlyCollection<int> itemIds, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        if (itemIds.Count == 0)
        {
            return;
        }

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            $"UPDATE users_items SET expires_at = @expiresAt WHERE id IN ({string.Join(", ", itemIds.Select((_, i) => $"@id{i}"))});", connection);
        command.Parameters.AddWithValue("@expiresAt", expiresAt);
        int index = 0;
        foreach (int itemId in itemIds)
        {
            command.Parameters.AddWithValue($"@id{index++}", itemId);
        }

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExpiredRental>> GetExpiredRentalsAsync(int limit, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT i.id, i.user_ref, i.room_ref, fd.type FROM users_items i
            INNER JOIN definition_furniture fd ON fd.id = i.ref_furni_def
            WHERE i.expires_at <= UTC_TIMESTAMP()
            ORDER BY i.expires_at
            LIMIT @limit;
            """, connection);
        command.Parameters.AddWithValue("@limit", limit);

        var expired = new List<ExpiredRental>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            expired.Add(new ExpiredRental(
                reader.GetInt32(0), reader.GetInt32(1), reader.IsDBNull(2) ? null : reader.GetInt32(2),
                reader.GetString(3) == "s" ? FurnitureType.Floor : FurnitureType.Wall));
        }

        return expired;
    }

    public async Task<bool> DeleteExpiredRentalAsync(int itemId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "DELETE FROM users_items WHERE id = @id AND expires_at <= UTC_TIMESTAMP();", connection);
        command.Parameters.AddWithValue("@id", itemId);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private static UserItem MapRow(MySqlDataReader reader) =>
        new(
            Id: reader.GetInt32(0),
            UserId: reader.GetInt32(1),
            FurniDefId: reader.GetInt32(2),
            RefFurnidata: reader.GetInt32(3),
            Classname: reader.GetString(4),
            Type: reader.GetString(5) == "s" ? FurnitureType.Floor : FurnitureType.Wall,
            LogicClass: FurnitureLogicClassExtensions.FromDbString(reader.GetString(6)),
            AllowTrade: reader.GetBoolean(7),
            AllowRecycle: reader.GetBoolean(8),
            ExtraData: reader.IsDBNull(9) ? null : reader.GetString(9),
            RoomRef: reader.IsDBNull(10) ? null : reader.GetInt32(10),
            X: reader.IsDBNull(11) ? null : reader.GetInt32(11),
            Y: reader.IsDBNull(12) ? null : reader.GetInt32(12),
            Z: reader.IsDBNull(13) ? null : reader.GetDecimal(13),
            Rotation: reader.IsDBNull(14) ? null : reader.GetInt32(14),
            WallPosition: reader.IsDBNull(15) ? null : reader.GetString(15),
            Seen: reader.GetBoolean(16),
            Extras: reader.IsDBNull(17) ? null : reader.GetInt32(17),
            ExpiresAt: reader.IsDBNull(18) ? null : DateTime.SpecifyKind(reader.GetDateTime(18), DateTimeKind.Utc),
            OnMarketplace: reader.GetBoolean(19));
}
