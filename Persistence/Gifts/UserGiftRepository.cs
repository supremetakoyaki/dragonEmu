using System.Text.Json;
using System.Text.Json.Serialization;
using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class UserGiftRepository : IUserGiftRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserGiftRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateAsync(int itemId, int senderUserId, IReadOnlyList<GiftContent> contents, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "INSERT INTO users_gifts (ref_item, sender_ref, contents) VALUES (@itemId, @senderUserId, @contents);",
            connection);
        command.Parameters.AddWithValue("@itemId", itemId);
        command.Parameters.AddWithValue("@senderUserId", senderUserId);
        command.Parameters.AddWithValue("@contents", SerializeContents(contents));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<UserGift?> GetByItemIdAsync(int itemId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT ref_item, sender_ref, contents FROM users_gifts WHERE ref_item = @itemId;", connection);
        command.Parameters.AddWithValue("@itemId", itemId);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UserGift(
            reader.GetInt32(0),
            reader.IsDBNull(1) ? null : reader.GetInt32(1),
            DeserializeContents(reader.GetString(2)));
    }

    public async Task DeleteAsync(int itemId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "DELETE FROM users_gifts WHERE ref_item = @itemId;", connection);
        command.Parameters.AddWithValue("@itemId", itemId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    internal static string SerializeContents(IReadOnlyList<GiftContent> contents)
    {
        var rows = contents.Select(c => new GiftContentRow(c.Type.ToJsonString(), c.Reference, c.ExtraData));
        return JsonSerializer.Serialize(rows);
    }

    private static IReadOnlyList<GiftContent> DeserializeContents(string json)
    {
        List<GiftContentRow>? rows = JsonSerializer.Deserialize<List<GiftContentRow>>(json);
        if (rows is null)
        {
            return [];
        }

        return rows
            .Select(r => new GiftContent(GiftContentTypeExtensions.FromJsonString(r.Type), r.Reference, r.ExtraData))
            .ToList();
    }

    private sealed record GiftContentRow(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("reference")] string Reference,
        [property: JsonPropertyName("extra_data")] string ExtraData);
}
