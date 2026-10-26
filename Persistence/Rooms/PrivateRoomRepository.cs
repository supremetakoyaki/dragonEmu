using System.Text.Json;
using System.Text.Json.Serialization;
using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class PrivateRoomRepository : IPrivateRoomRepository
{
    private const string BaseSelect =
        """
        SELECT pr.id, pr.label, pr.description, pr.category_ref, pr.access_type, pr.max_users,
               pr.password, pr.owner_ref, COALESCE(u.username, ''), pr.score,
               pr.owner_ref IS NOT NULL AND COALESCE(s.allow_trading, 1), COALESCE(s.allow_pets, 0),
               COALESCE(i.imgid, 1), COALESCE(i.foreground, 1), i.items,
               dh.heightmap, dh.door_x, dh.door_y, dh.door_z, dh.door_dir,
               pr.wallpaper, pr.floor, pr.landscape, dh.label,
               COALESCE(s.allow_pets_eat, 0), COALESCE(s.allow_walkthrough, 0),
               CAST(COALESCE(s.queuetile_speed, '1') AS SIGNED), dh.max_step_height, dh.pool_water_max_height
        FROM rooms pr
        LEFT JOIN users u ON u.id = pr.owner_ref
        INNER JOIN definition_room_heightmap dh ON dh.id = pr.heightmap_ref
        LEFT JOIN users_private_rooms_settings s ON s.room_ref = pr.id
        LEFT JOIN users_private_rooms_icons i ON i.room_ref = pr.id
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public PrivateRoomRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private const string OccupiedFirst = "ORDER BY FIND_IN_SET(pr.id, @occupied) = 0, pr.score DESC, pr.id LIMIT @limit;";

    public Task<IReadOnlyList<PrivateRoomSummary>> GetOwnedByAsync(int userId, CancellationToken cancellationToken = default) =>
        GetListAsync(
            $"{BaseSelect} WHERE pr.owner_ref = @userId ORDER BY pr.id ASC;",
            parameters => parameters.AddWithValue("@userId", userId), cancellationToken);

    public Task<IReadOnlyList<PrivateRoomSummary>> GetFavouritedByAsync(int userId, CancellationToken cancellationToken = default) =>
        GetListAsync(
            $"{BaseSelect} INNER JOIN users_favourite_rooms f ON f.room_ref = pr.id WHERE f.user_ref = @userId ORDER BY f.created_at, pr.id;",
            parameters => parameters.AddWithValue("@userId", userId), cancellationToken);

    public Task<IReadOnlyList<PrivateRoomSummary>> GetVisitedByAsync(int userId, int limit, CancellationToken cancellationToken = default) =>
        GetListAsync(
            $"{BaseSelect} INNER JOIN users_visited_rooms v ON v.room_ref = pr.id WHERE v.user_ref = @userId ORDER BY v.last_visited_at DESC, pr.id LIMIT @limit;",
            parameters =>
            {
                parameters.AddWithValue("@userId", userId);
                parameters.AddWithValue("@limit", limit);
            },
            cancellationToken);

    public Task<IReadOnlyList<PrivateRoomSummary>> GetHighestScoredAsync(int limit, CancellationToken cancellationToken = default) =>
        GetListAsync(
            $"{BaseSelect} WHERE pr.owner_ref IS NOT NULL AND pr.score > 0 ORDER BY pr.score DESC, pr.id LIMIT @limit;",
            parameters => parameters.AddWithValue("@limit", limit), cancellationToken);

    public Task<IReadOnlyList<PrivateRoomSummary>> SearchByTextAsync(
        string text, IReadOnlyCollection<int> occupiedRoomIds, int limit, CancellationToken cancellationToken = default) =>
        GetListAsync(
            $"{BaseSelect} WHERE pr.owner_ref IS NOT NULL AND (pr.label LIKE @pattern OR u.username = @text) {OccupiedFirst}",
            parameters =>
            {
                parameters.AddWithValue("@pattern", $"%{SqlLike.Escape(text)}%");
                parameters.AddWithValue("@text", text);
                AddOccupiedFirst(parameters, occupiedRoomIds, limit);
            },
            cancellationToken);

    public Task<IReadOnlyList<PrivateRoomSummary>> GetOwnedByFriendsOfAsync(
        int userId, IReadOnlyCollection<int> occupiedRoomIds, int limit, CancellationToken cancellationToken = default) =>
        GetListAsync(
            $"{BaseSelect} WHERE pr.owner_ref IN (SELECT f.friend_ref FROM users_friends f WHERE f.user_ref = @userId) {OccupiedFirst}",
            parameters =>
            {
                parameters.AddWithValue("@userId", userId);
                AddOccupiedFirst(parameters, occupiedRoomIds, limit);
            },
            cancellationToken);

    public Task<IReadOnlyList<PrivateRoomSummary>> SearchByTagAsync(
        string tag, IReadOnlyCollection<int> occupiedRoomIds, int limit, CancellationToken cancellationToken = default) =>
        GetListAsync(
            $"""
            {BaseSelect}
            WHERE pr.owner_ref IS NOT NULL
              AND pr.id IN (SELECT t.room_ref FROM users_private_rooms_tags t WHERE t.tag = @tag)
            {OccupiedFirst}
            """,
            parameters =>
            {
                parameters.AddWithValue("@tag", tag);
                AddOccupiedFirst(parameters, occupiedRoomIds, limit);
            },
            cancellationToken);

    private static void AddOccupiedFirst(MySqlParameterCollection parameters, IReadOnlyCollection<int> occupiedRoomIds, int limit)
    {
        parameters.AddWithValue("@occupied", string.Join(',', occupiedRoomIds));
        parameters.AddWithValue("@limit", limit);
    }

    private async Task<IReadOnlyList<PrivateRoomSummary>> GetListAsync(
        string sql, Action<MySqlParameterCollection> bind, CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var rooms = new List<PrivateRoomSummary>();

        await using (var command = new MySqlCommand(sql, connection))
        {
            bind(command.Parameters);

            await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rooms.Add(MapRow(reader));
            }
        }

        return await AttachTagsAsync(connection, rooms, cancellationToken);
    }

    public async Task<PrivateRoomSummary?> GetByIdAsync(int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        PrivateRoomSummary? room = null;

        await using (var command = new MySqlCommand($"{BaseSelect} WHERE pr.id = @roomId LIMIT 1;", connection))
        {
            command.Parameters.AddWithValue("@roomId", roomId);

            await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                room = MapRow(reader);
            }
        }

        if (room is null)
        {
            return null;
        }

        IReadOnlyList<PrivateRoomSummary> withTags = await AttachTagsAsync(connection, [room], cancellationToken);
        return withTags[0];
    }

    public Task<IReadOnlyList<PrivateRoomSummary>> GetPublicAsync(CancellationToken cancellationToken = default) =>
        GetListAsync($"{BaseSelect} WHERE pr.owner_ref IS NULL;", _ => { }, cancellationToken);

    public async Task<int> CountOwnedByAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT COUNT(*) FROM rooms WHERE owner_ref = @userId;", connection);
        command.Parameters.AddWithValue("@userId", userId);

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    public async Task<PrivateRoomSummary> CreateAsync(
        int ownerId, string label, string description, int categoryId, int heightmapId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        int roomId;
        await using (var insertRoom = new MySqlCommand(
            """
            INSERT INTO rooms (label, description, category_ref, heightmap_ref, owner_ref, access_type)
            VALUES (@label, @description, @categoryId, @heightmapId, @ownerId, 0);
            """, connection))
        {
            insertRoom.Parameters.AddWithValue("@label", label);
            insertRoom.Parameters.AddWithValue("@description", description);
            insertRoom.Parameters.AddWithValue("@categoryId", categoryId);
            insertRoom.Parameters.AddWithValue("@heightmapId", heightmapId);
            insertRoom.Parameters.AddWithValue("@ownerId", ownerId);
            await insertRoom.ExecuteNonQueryAsync(cancellationToken);
            roomId = (int)insertRoom.LastInsertedId;
        }

        return await GetByIdAsync(roomId, cancellationToken)
            ?? throw new InvalidOperationException($"Just-created room {roomId} could not be reloaded.");
    }

    public async Task<PrivateRoomSummary> UpdateSettingsAsync(
        int roomId, string label, string description, int categoryId, int accessType, string? password, int maxUsers,
        IReadOnlyList<string> tags, bool allowPets, bool allowPetsEat, bool allowWalkthrough, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var updateRoom = new MySqlCommand(
            """
            UPDATE rooms SET label = @label, description = @description, category_ref = @categoryId,
                              access_type = @accessType, password = @password, max_users = @maxUsers
            WHERE id = @roomId;
            """, connection, transaction))
        {
            updateRoom.Parameters.AddWithValue("@label", label);
            updateRoom.Parameters.AddWithValue("@description", description);
            updateRoom.Parameters.AddWithValue("@categoryId", categoryId);
            updateRoom.Parameters.AddWithValue("@accessType", accessType);
            updateRoom.Parameters.AddWithValue("@password", (object?)password ?? DBNull.Value);
            updateRoom.Parameters.AddWithValue("@maxUsers", maxUsers);
            updateRoom.Parameters.AddWithValue("@roomId", roomId);
            await updateRoom.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var upsertSettings = new MySqlCommand(
            """
            INSERT INTO users_private_rooms_settings (room_ref, allow_pets, allow_pets_eat, allow_walkthrough)
            VALUES (@roomId, @allowPets, @allowPetsEat, @allowWalkthrough)
            ON DUPLICATE KEY UPDATE allow_pets = VALUES(allow_pets), allow_pets_eat = VALUES(allow_pets_eat),
                                     allow_walkthrough = VALUES(allow_walkthrough);
            """, connection, transaction))
        {
            upsertSettings.Parameters.AddWithValue("@roomId", roomId);
            upsertSettings.Parameters.AddWithValue("@allowPets", allowPets);
            upsertSettings.Parameters.AddWithValue("@allowPetsEat", allowPetsEat);
            upsertSettings.Parameters.AddWithValue("@allowWalkthrough", allowWalkthrough);
            await upsertSettings.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var deleteTags = new MySqlCommand("DELETE FROM users_private_rooms_tags WHERE room_ref = @roomId;", connection, transaction))
        {
            deleteTags.Parameters.AddWithValue("@roomId", roomId);
            await deleteTags.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (string tag in tags)
        {
            await using var insertTag = new MySqlCommand(
                "INSERT INTO users_private_rooms_tags (room_ref, tag) VALUES (@roomId, @tag);", connection, transaction);
            insertTag.Parameters.AddWithValue("@roomId", roomId);
            insertTag.Parameters.AddWithValue("@tag", tag);
            await insertTag.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        return await GetByIdAsync(roomId, cancellationToken)
            ?? throw new InvalidOperationException($"Just-updated room {roomId} could not be reloaded.");
    }

    public async Task SaveIconAsync(
        int roomId, int background, int foreground, IReadOnlyList<RoomIconItem> items, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            INSERT INTO users_private_rooms_icons (room_ref, imgid, foreground, items)
            VALUES (@roomId, @background, @foreground, @items)
            ON DUPLICATE KEY UPDATE imgid = VALUES(imgid), foreground = VALUES(foreground), items = VALUES(items);
            """, connection);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@background", background);
        command.Parameters.AddWithValue("@foreground", foreground);
        command.Parameters.AddWithValue("@items", items.Count == 0
            ? DBNull.Value
            : JsonSerializer.Serialize(items.Select(item => new IconItemDto(item.Position, item.ItemId))));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SetQueuetileSpeedAsync(int roomId, int speed, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new MySqlCommand(
            """
            INSERT INTO users_private_rooms_settings (room_ref, queuetile_speed)
            VALUES (@roomId, @speed)
            ON DUPLICATE KEY UPDATE queuetile_speed = VALUES(queuetile_speed);
            """, connection);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@speed", speed.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>?> DeleteAsync(int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        async Task RunAsync(string sql)
        {
            await using var command = new MySqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@roomId", roomId);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var owned = new MySqlCommand(
            "SELECT 1 FROM rooms WHERE id = @roomId AND owner_ref IS NOT NULL FOR UPDATE;", connection, transaction))
        {
            owned.Parameters.AddWithValue("@roomId", roomId);
            if (await owned.ExecuteScalarAsync(cancellationToken) is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return null;
            }
        }

        var returnedTo = new List<int>();
        await using (var owners = new MySqlCommand(
            """
            SELECT DISTINCT i.user_ref FROM users_items i
            INNER JOIN definition_furniture fd ON fd.id = i.ref_furni_def
            WHERE i.room_ref = @roomId AND fd.logic_class <> 'stickie';
            """, connection, transaction))
        {
            owners.Parameters.AddWithValue("@roomId", roomId);
            await using MySqlDataReader reader = await owners.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                returnedTo.Add(reader.GetInt32(0));
            }
        }

        await RunAsync(
            """
            DELETE i FROM users_items i
            INNER JOIN definition_furniture fd ON fd.id = i.ref_furni_def
            WHERE i.room_ref = @roomId AND fd.logic_class = 'stickie';
            """);
        await RunAsync(
            """
            UPDATE users_items SET room_ref = NULL, x = NULL, y = NULL, z = NULL, rotation = NULL, wall_position = NULL
            WHERE room_ref = @roomId;
            """);
        await RunAsync("UPDATE users_settings SET home_room = 0 WHERE home_room = @roomId;");
        await RunAsync("DELETE FROM rooms WHERE id = @roomId;");

        await transaction.CommitAsync(cancellationToken);
        return returnedTo;
    }

    public async Task<int> LockRoomsWithBrokenPasswordsAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "UPDATE rooms SET access_type = @doorbell, password = NULL " +
            "WHERE access_type = @password AND owner_ref IS NOT NULL " +
            "AND (password IS NULL OR NOT REGEXP_LIKE(password, '^[0-9a-f]{40}$', 'c'));", connection);
        command.Parameters.AddWithValue("@doorbell", RoomAccess.Doorbell);
        command.Parameters.AddWithValue("@password", RoomAccess.Password);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdatePaintAsync(int roomId, RoomPaintType type, string value, CancellationToken cancellationToken = default)
    {
        string column = type switch
        {
            RoomPaintType.Wallpaper => "wallpaper",
            RoomPaintType.Floor => "floor",
            RoomPaintType.Landscape => "landscape",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand($"UPDATE rooms SET {column} = @value WHERE id = @roomId;", connection);
        command.Parameters.AddWithValue("@value", value);
        command.Parameters.AddWithValue("@roomId", roomId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static PrivateRoomSummary MapRow(MySqlDataReader reader)
    {
        IReadOnlyList<RoomIconItem> iconItems = reader.IsDBNull(14) ? [] : ParseIconItems(reader.GetString(14));
        int? ownerId = reader.IsDBNull(7) ? null : reader.GetInt32(7);
        (int accessType, string? passwordHash) = RoomAccess.Resolve(ownerId, reader.GetInt32(4), reader.IsDBNull(6) ? null : reader.GetString(6));

        return new PrivateRoomSummary(
            Id: reader.GetInt32(0),
            Label: reader.GetString(1),
            Description: reader.GetString(2),
            CategoryId: reader.GetInt32(3),
            AccessType: accessType,
            Password: passwordHash,
            OwnerId: ownerId,
            OwnerUsername: reader.GetString(8),
            MaxUsers: reader.GetInt32(5),
            AllowTrading: reader.GetInt32(10) != 0,
            AllowPets: reader.GetInt32(11) != 0,
            Score: reader.GetInt32(9),
            Tags: [],
            IconBackground: reader.GetInt32(12),
            IconForeground: reader.GetInt32(13),
            IconItems: iconItems,
            Heightmap: reader.GetString(15),
            DoorX: reader.GetInt32(16),
            DoorY: reader.GetInt32(17),
            DoorZ: reader.GetDouble(18),
            DoorDir: reader.GetInt32(19),
            Wallpaper: reader.GetString(20),
            Floor: reader.GetString(21),
            Landscape: reader.GetString(22),
            HeightmapLabel: reader.GetString(23),
            AllowPetsEat: reader.GetInt32(24) != 0,
            AllowWalkthrough: reader.GetInt32(25) != 0,
            QueuetileSpeed: Convert.ToInt32(reader.GetValue(26)),
            MaxStepHeight: reader.IsDBNull(27) ? null : reader.GetDouble(27),
            PoolWaterMaxHeight: reader.IsDBNull(28) ? null : reader.GetDouble(28));
    }

    private static async Task<List<PrivateRoomSummary>> AttachTagsAsync(
        MySqlConnection connection, List<PrivateRoomSummary> rooms, CancellationToken cancellationToken)
    {
        if (rooms.Count == 0)
        {
            return rooms;
        }

        Dictionary<int, List<string>> tagsByRoomId = await LoadTagsAsync(connection, rooms.Select(r => r.Id), cancellationToken);

        for (int i = 0; i < rooms.Count; i++)
        {
            PrivateRoomSummary room = rooms[i];
            if (tagsByRoomId.TryGetValue(room.Id, out List<string>? tags))
            {
                rooms[i] = room with { Tags = tags };
            }
        }

        return rooms;
    }

    private static async Task<Dictionary<int, List<string>>> LoadTagsAsync(
        MySqlConnection connection, IEnumerable<int> roomIds, CancellationToken cancellationToken)
    {
        string[] ids = roomIds.Select(id => id.ToString()).ToArray();
        var tagsByRoomId = new Dictionary<int, List<string>>();

        await using var command = new MySqlCommand(
            $"SELECT room_ref, tag FROM users_private_rooms_tags WHERE room_ref IN ({string.Join(',', ids)});",
            connection);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            int roomId = reader.GetInt32(0);
            string tag = reader.GetString(1);

            if (!tagsByRoomId.TryGetValue(roomId, out List<string>? tags))
            {
                tags = [];
                tagsByRoomId[roomId] = tags;
            }

            tags.Add(tag);
        }

        return tagsByRoomId;
    }

    private static IReadOnlyList<RoomIconItem> ParseIconItems(string json)
    {
        List<IconItemDto>? items = JsonSerializer.Deserialize<List<IconItemDto>>(json);
        return items is null ? [] : items.Select(i => new RoomIconItem(i.Position, i.Id)).ToList();
    }

    private sealed record IconItemDto(
        [property: JsonPropertyName("position")] int Position,
        [property: JsonPropertyName("id")] int Id);
}
