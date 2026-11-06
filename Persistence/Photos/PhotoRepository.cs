using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class PhotoRepository : IPhotoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PhotoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(
        int userId, int photoDefinitionId, int roomId, string caption, string token, byte[] png, DateTime takenAt,
        CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        int itemId;
        await using (var item = new MySqlCommand(
            "INSERT INTO users_items (user_ref, ref_furni_def, extra_data, seen) VALUES (@userId, @furniDefId, '', FALSE);",
            connection, transaction))
        {
            item.Parameters.AddWithValue("@userId", userId);
            item.Parameters.AddWithValue("@furniDefId", photoDefinitionId);
            await item.ExecuteNonQueryAsync(cancellationToken);
            itemId = (int)item.LastInsertedId;
        }

        await using (var photo = new MySqlCommand(
            """
            INSERT INTO photos (item_ref, user_ref, room_ref, caption, token, image, taken_at)
            VALUES (@itemId, @userId, @roomId, @caption, @token, @image, @takenAt);
            """, connection, transaction))
        {
            photo.Parameters.AddWithValue("@itemId", itemId);
            photo.Parameters.AddWithValue("@userId", userId);
            photo.Parameters.AddWithValue("@roomId", roomId);
            photo.Parameters.AddWithValue("@caption", caption);
            photo.Parameters.AddWithValue("@token", token);
            photo.Parameters.AddWithValue("@image", png);
            photo.Parameters.AddWithValue("@takenAt", takenAt);
            await photo.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return itemId;
    }

    public async Task<Photo?> GetByItemAsync(int itemId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT id, item_ref, user_ref, caption, token, taken_at, removed_at IS NOT NULL FROM photos WHERE item_ref = @itemId;",
            connection);
        command.Parameters.AddWithValue("@itemId", itemId);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Photo(
            Id: reader.GetInt32(0), ItemId: reader.GetInt32(1), UserId: reader.GetInt32(2), Caption: reader.GetString(3),
            Token: reader.GetString(4), TakenAt: DateTime.SpecifyKind(reader.GetDateTime(5), DateTimeKind.Utc), Removed: reader.GetBoolean(6));
    }
}
