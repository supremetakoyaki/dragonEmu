using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class RoomAdRepository : IRoomAdRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoomAdRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<RoomAd>> GetForRoomAsync(int roomId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        IReadOnlyList<RoomAd> own = await ReadAsync(connection, "ref_room = @roomId", roomId, cancellationToken);
        return own.Count > 0 ? own : await ReadAsync(connection, "ref_room IS NULL", roomId, cancellationToken);
    }

    private static async Task<IReadOnlyList<RoomAd>> ReadAsync(MySqlConnection connection, string where, int roomId, CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand(
            $"SELECT image_url, link FROM definition_room_ads WHERE enabled = '1' AND {where} ORDER BY id;", connection);
        command.Parameters.AddWithValue("@roomId", roomId);

        var ads = new List<RoomAd>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            ads.Add(new RoomAd(reader.GetString(0), reader.GetString(1)));
        }

        return ads;
    }
}
