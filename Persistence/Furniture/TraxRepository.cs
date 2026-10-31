using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class TraxRepository : ITraxRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TraxRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<TraxSong>> GetPlaylistAsync(int machineItemId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT s.id, s.song_name, s.song_author, s.song_data
            FROM sound_machine_playlists p
            INNER JOIN users_songs s ON s.item_ref = p.disk_item_ref
            INNER JOIN users_items disk ON disk.id = p.disk_item_ref
            INNER JOIN definition_furniture d ON d.id = disk.ref_furni_def AND d.interaction_logic = 'sound_disk'
            WHERE p.machine_item_ref = @machine
            ORDER BY p.position;
            """, connection);
        command.Parameters.AddWithValue("@machine", machineItemId);

        var songs = new List<TraxSong>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            songs.Add(new TraxSong(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
        }

        return songs;
    }

    public async Task<IReadOnlyList<TraxSong>> GetUserSongsAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT id, song_name, song_author, song_data FROM users_songs WHERE user_ref = @user ORDER BY song_name, id;", connection);
        command.Parameters.AddWithValue("@user", userId);

        var songs = new List<TraxSong>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            songs.Add(new TraxSong(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
        }

        return songs;
    }
}
