using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class PetRepository : IPetRepository
{
    private const string BaseSelect =
        """
        SELECT p.id, p.user_ref, COALESCE(u.username, ''), p.type, p.breed, p.color, p.name, p.room_ref,
               COALESCE(p.x, 0), COALESCE(p.y, 0), COALESCE(p.z, 0), p.respect_amount, p.level, p.experience, p.energy,
               p.nutrition, p.created_at
        FROM users_pets p
        LEFT JOIN users u ON u.id = p.user_ref
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public PetRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<IReadOnlyList<PetRecord>> GetInventoryAsync(int ownerId, CancellationToken cancellationToken = default) =>
        ReadAsync($"{BaseSelect} WHERE p.user_ref = @id AND p.room_ref IS NULL ORDER BY p.id;", ownerId, cancellationToken);

    public Task<IReadOnlyList<PetRecord>> GetInRoomAsync(int roomId, CancellationToken cancellationToken = default) =>
        ReadAsync($"{BaseSelect} WHERE p.room_ref = @id ORDER BY p.id;", roomId, cancellationToken);

    public async Task<PetRecord?> GetByIdAsync(int petId, CancellationToken cancellationToken = default) =>
        (await ReadAsync($"{BaseSelect} WHERE p.id = @id;", petId, cancellationToken)).FirstOrDefault();

    public async Task<int> CreateAsync(
        int ownerId, int type, int breed, string color, string name, int energy, int nutrition, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            INSERT INTO users_pets (user_ref, type, breed, color, name, level, experience, energy, nutrition)
            VALUES (@ownerId, @type, @breed, @color, @name, 1, 0, @energy, @nutrition);
            """, connection);
        command.Parameters.AddWithValue("@ownerId", ownerId);
        command.Parameters.AddWithValue("@type", type);
        command.Parameters.AddWithValue("@breed", breed);
        command.Parameters.AddWithValue("@color", color);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@energy", energy);
        command.Parameters.AddWithValue("@nutrition", nutrition);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return (int)command.LastInsertedId;
    }

    public async Task<bool> PlaceAsync(int petId, int ownerId, int roomId, int x, int y, double z, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "UPDATE users_pets SET room_ref = @roomId, x = @x, y = @y, z = @z WHERE id = @petId AND user_ref = @ownerId AND room_ref IS NULL;",
            connection);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@x", x);
        command.Parameters.AddWithValue("@y", y);
        command.Parameters.AddWithValue("@z", z);
        command.Parameters.AddWithValue("@petId", petId);
        command.Parameters.AddWithValue("@ownerId", ownerId);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> ReturnToInventoryAsync(int petId, PetStats stats, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            UPDATE users_pets
            SET room_ref = NULL, x = NULL, y = NULL, z = NULL,
                level = @level, experience = @experience, energy = @energy, nutrition = @nutrition
            WHERE id = @petId AND room_ref IS NOT NULL;
            """, connection);
        command.Parameters.AddWithValue("@petId", petId);
        BindStats(command, stats);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<int> ReturnStrayPetsAsync(int roomId, int? roomOwnerId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            UPDATE users_pets SET room_ref = NULL, x = NULL, y = NULL, z = NULL
            WHERE room_ref = @roomId AND (@ownerId IS NULL OR user_ref <> @ownerId);
            """, connection);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@ownerId", roomOwnerId);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveAsync(IReadOnlyCollection<PetSave> pets, CancellationToken cancellationToken = default)
    {
        if (pets.Count == 0)
        {
            return;
        }

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        foreach (PetSave pet in pets)
        {
            await using var command = new MySqlCommand(
                """
                UPDATE users_pets
                SET x = @x, y = @y, z = @z, level = @level, experience = @experience, energy = @energy, nutrition = @nutrition
                WHERE id = @petId AND room_ref IS NOT NULL;
                """, connection, transaction);
            command.Parameters.AddWithValue("@petId", pet.PetId);
            command.Parameters.AddWithValue("@x", pet.X);
            command.Parameters.AddWithValue("@y", pet.Y);
            command.Parameters.AddWithValue("@z", pet.Z);
            BindStats(command, pet.Stats);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<int> CountOwnedAsync(int ownerId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT COUNT(*) FROM users_pets WHERE user_ref = @ownerId;", connection);
        command.Parameters.AddWithValue("@ownerId", ownerId);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<int?> AddRespectAsync(int petId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using (var update = new MySqlCommand("UPDATE users_pets SET respect_amount = respect_amount + 1 WHERE id = @petId;", connection))
        {
            update.Parameters.AddWithValue("@petId", petId);
            if (await update.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                return null;
            }
        }

        await using var read = new MySqlCommand("SELECT respect_amount FROM users_pets WHERE id = @petId;", connection);
        read.Parameters.AddWithValue("@petId", petId);
        return Convert.ToInt32(await read.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<bool> TryUsePetRespectAsync(int userId, DateOnly today, int dailyGrant, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new MySqlCommand(
            """
            UPDATE users
            SET pet_respects_given_today = IF(pet_respect_day = @today, pet_respects_given_today, 0) + 1,
                pet_respect_day = @today
            WHERE id = @userId
              AND (pet_respect_day IS NULL OR pet_respect_day <> @today OR pet_respects_given_today < @dailyGrant);
            """, connection);
        command.Parameters.AddWithValue("@today", today.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@dailyGrant", dailyGrant);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private async Task<IReadOnlyList<PetRecord>> ReadAsync(string sql, int id, CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        var pets = new List<PetRecord>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            pets.Add(new PetRecord(
                Id: reader.GetInt32(0),
                OwnerId: reader.GetInt32(1),
                OwnerName: reader.GetString(2),
                Type: reader.GetInt32(3),
                Breed: reader.GetInt32(4),
                Color: reader.GetString(5),
                Name: reader.GetString(6),
                RoomId: reader.IsDBNull(7) ? null : reader.GetInt32(7),
                X: Convert.ToInt32(reader.GetValue(8)),
                Y: Convert.ToInt32(reader.GetValue(9)),
                Z: Convert.ToDouble(reader.GetValue(10)),
                Respect: reader.GetInt32(11),
                Level: reader.GetInt32(12),
                Experience: reader.GetInt32(13),
                Energy: reader.GetInt32(14),
                Nutrition: reader.GetInt32(15),
                CreatedAt: DateTime.SpecifyKind(reader.GetDateTime(16), DateTimeKind.Utc)));
        }

        return pets;
    }

    private static void BindStats(MySqlCommand command, PetStats stats)
    {
        command.Parameters.AddWithValue("@level", stats.Level);
        command.Parameters.AddWithValue("@experience", stats.Experience);
        command.Parameters.AddWithValue("@energy", stats.Energy);
        command.Parameters.AddWithValue("@nutrition", stats.Nutrition);
    }
}

public sealed class PetDefinitionRepository : IPetDefinitionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PetDefinitionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<IReadOnlyList<PetLevel>> GetLevelsAsync(CancellationToken cancellationToken = default) =>
        ReadAsync("SELECT level, experience_cap, nutrition_cap, energy_cap FROM definition_pet_levels ORDER BY level;",
            reader => new PetLevel(Int(reader, 0), Int(reader, 1), Int(reader, 2), Int(reader, 3)), cancellationToken);

    public Task<IReadOnlyList<PetCommand>> GetCommandsAsync(CancellationToken cancellationToken = default) =>
        ReadAsync("SELECT id, name, required_level, experience, energy_cost, nutrition_cost FROM definition_pet_commands ORDER BY id;",
            reader => new PetCommand(Int(reader, 0), reader.GetString(1), Int(reader, 2), Int(reader, 3), Int(reader, 4), Int(reader, 5)), cancellationToken);

    public Task<IReadOnlyList<(int Type, PetSpeechType Kind, string Text)>> GetSpeechAsync(CancellationToken cancellationToken = default) =>
        ReadAsync("SELECT pet_type, speech_type, text FROM definition_pet_speech ORDER BY id;",
            reader => (Int(reader, 0), KindOf(reader.GetString(1)), reader.GetString(2)), cancellationToken);

    public Task<IReadOnlyList<(int Type, int Breed)>> GetBreedsAsync(CancellationToken cancellationToken = default) =>
        ReadAsync("SELECT pet_type, breed FROM definition_pet_breeds;", reader => (Int(reader, 0), Int(reader, 1)), cancellationToken);

    public Task<IReadOnlyList<(int Type, string Color)>> GetColorsAsync(CancellationToken cancellationToken = default) =>
        ReadAsync("SELECT pet_type, color FROM definition_pet_colors;", reader => (Int(reader, 0), reader.GetString(1)), cancellationToken);

    public Task<IReadOnlyList<PetFood>> GetFoodAsync(CancellationToken cancellationToken = default) =>
        ReadAsync("SELECT furni_def_ref, portions, full_state, nutrition, energy, experience FROM definition_pet_food;",
            reader => new PetFood(Int(reader, 0), Int(reader, 1), Int(reader, 2), Int(reader, 3), Int(reader, 4), Int(reader, 5)), cancellationToken);

    private static PetSpeechType KindOf(string value) => value switch
    {
        "training_positive" => PetSpeechType.TrainingPositive,
        "training_negative" => PetSpeechType.TrainingNegative,
        "eating" => PetSpeechType.Eating,
        "playful" => PetSpeechType.Playful,
        "drinking" => PetSpeechType.Drinking,
        "sleeping" => PetSpeechType.Sleeping,
        "welcome_home" => PetSpeechType.WelcomeHome,
        _ => PetSpeechType.RandomSpeech,
    };

    private static int Int(MySqlDataReader reader, int index) => Convert.ToInt32(reader.GetValue(index));

    private async Task<IReadOnlyList<T>> ReadAsync<T>(string sql, Func<MySqlDataReader, T> map, CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        var rows = new List<T>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(map(reader));
        }

        return rows;
    }
}
