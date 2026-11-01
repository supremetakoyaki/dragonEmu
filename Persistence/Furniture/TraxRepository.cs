using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class TraxRepository : ITraxRepository
{
    private const string SongColumns =
        "s.id, s.user_ref, s.song_name, s.song_author, s.song_data, EXISTS (SELECT 1 FROM users_songs_disks b WHERE b.song_ref = s.id)";

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
            FROM trax_machine_playlists p
            INNER JOIN users_songs s ON s.id = p.song_ref
            WHERE p.machine_item_ref = @machine
            ORDER BY p.position;
            """, connection);
        command.Parameters.AddWithValue("@machine", machineItemId);
        return await ReadSongsAsync(command, cancellationToken);
    }

    public async Task<IReadOnlyList<TraxSong>> GetJukeboxPlaylistAsync(int jukeboxItemId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT s.id, s.song_name, s.song_author, s.song_data
            FROM jukebox_playlists p
            INNER JOIN users_songs_disks d ON d.disk_item_ref = p.disk_item_ref
            INNER JOIN users_songs s ON s.id = d.song_ref
            WHERE p.machine_item_ref = @machine
            ORDER BY p.position;
            """, connection);
        command.Parameters.AddWithValue("@machine", jukeboxItemId);
        return await ReadSongsAsync(command, cancellationToken);
    }

    public async Task<IReadOnlyList<TraxSong>> GetUserSongsAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT id, song_name, song_author, song_data FROM users_songs WHERE user_ref = @user AND deleted_at IS NULL ORDER BY song_name, id;",
            connection);
        command.Parameters.AddWithValue("@user", userId);
        return await ReadSongsAsync(command, cancellationToken);
    }

    public async Task<IReadOnlyList<TraxSongEntry>> GetSongListAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            $"SELECT {SongColumns} FROM users_songs s WHERE s.user_ref = @user AND s.deleted_at IS NULL ORDER BY s.song_name, s.id;", connection);
        command.Parameters.AddWithValue("@user", userId);

        var songs = new List<TraxSongEntry>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            songs.Add(ReadEntry(reader));
        }

        return songs;
    }

    public async Task<TraxSongEntry?> GetSongAsync(int songId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand($"SELECT {SongColumns} FROM users_songs s WHERE s.id = @song AND s.deleted_at IS NULL;", connection);
        command.Parameters.AddWithValue("@song", songId);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadEntry(reader) : null;
    }

    public async Task<int> CountSongsAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT COUNT(*) FROM users_songs WHERE user_ref = @user AND deleted_at IS NULL;", connection);
        command.Parameters.AddWithValue("@user", userId);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<int> CreateSongAsync(int userId, string name, string author, string data, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "INSERT INTO users_songs (user_ref, song_name, song_author, song_data) VALUES (@user, @name, @author, @data); SELECT LAST_INSERT_ID();",
            connection);
        command.Parameters.AddWithValue("@user", userId);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@author", author);
        command.Parameters.AddWithValue("@data", data);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task UpdateSongAsync(int songId, string name, string data, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("UPDATE users_songs SET song_name = @name, song_data = @data WHERE id = @song;", connection);
        command.Parameters.AddWithValue("@song", songId);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@data", data);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteSongAsync(int songId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            DELETE FROM trax_machine_playlists WHERE song_ref = @song;
            UPDATE users_songs SET deleted_at = UTC_TIMESTAMP() WHERE id = @song AND EXISTS (SELECT 1 FROM users_songs_disks d WHERE d.song_ref = @song);
            DELETE FROM users_songs WHERE id = @song AND deleted_at IS NULL;
            """, connection);
        command.Parameters.AddWithValue("@song", songId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> HasBurntSongNamedAsync(int userId, string name, int exceptSongId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT EXISTS (
                SELECT 1 FROM users_songs s
                WHERE s.user_ref = @user AND s.song_name = @name AND s.id <> @except
                  AND EXISTS (SELECT 1 FROM users_songs_disks d WHERE d.song_ref = s.id));
            """, connection);
        command.Parameters.AddWithValue("@user", userId);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@except", exceptSongId);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) == 1;
    }

    public async Task<IReadOnlyList<int>> GetPlaylistSongIdsAsync(int machineItemId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT song_ref FROM trax_machine_playlists WHERE machine_item_ref = @machine ORDER BY position;", connection);
        command.Parameters.AddWithValue("@machine", machineItemId);
        return await ReadIdsAsync(command, cancellationToken);
    }

    public Task SetPlaylistAsync(int machineItemId, IReadOnlyList<int> songIds, CancellationToken cancellationToken = default) =>
        ReplaceListAsync("trax_machine_playlists", "song_ref", machineItemId, songIds, cancellationToken);

    public async Task<IReadOnlyList<TraxMachineSoundSet>> GetMachineSoundSetsAsync(int machineItemId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT h.slot, ds.id, i.id, i.user_ref
            FROM trax_machine_sound_sets h
            INNER JOIN users_items i ON i.id = h.sound_set_item_ref
            INNER JOIN definition_trax_sound_sets ds ON ds.furni_def_ref = i.ref_furni_def
            WHERE h.machine_item_ref = @machine
            ORDER BY h.slot;
            """, connection);
        command.Parameters.AddWithValue("@machine", machineItemId);

        var sets = new List<TraxMachineSoundSet>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            sets.Add(new TraxMachineSoundSet(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3)));
        }

        return sets;
    }

    public async Task<IReadOnlyList<(int ItemId, int SoundSetId)>> GetInventorySoundSetsAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            $"""
            SELECT i.id, ds.id
            FROM users_items i
            INNER JOIN definition_trax_sound_sets ds ON ds.furni_def_ref = i.ref_furni_def
            WHERE i.user_ref = @user AND i.room_ref IS NULL AND {ItemHolds.NotHeld("i.id")}
            ORDER BY ds.id, i.id;
            """, connection);
        command.Parameters.AddWithValue("@user", userId);

        var sets = new List<(int, int)>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            sets.Add((reader.GetInt32(0), reader.GetInt32(1)));
        }

        return sets;
    }

    public async Task<int?> InsertSoundSetAsync(int machineItemId, int slot, int userId, int soundSetId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        int itemId;
        await using (var select = new MySqlCommand(
            $"""
            SELECT i.id
            FROM users_items i
            INNER JOIN definition_trax_sound_sets ds ON ds.furni_def_ref = i.ref_furni_def
            WHERE i.user_ref = @user AND ds.id = @set AND i.room_ref IS NULL AND {ItemHolds.NotHeld("i.id")}
            ORDER BY i.id
            LIMIT 1
            FOR UPDATE OF i;
            """, connection, transaction))
        {
            select.Parameters.AddWithValue("@user", userId);
            select.Parameters.AddWithValue("@set", soundSetId);
            if (await select.ExecuteScalarAsync(cancellationToken) is not { } found)
            {
                return null;
            }

            itemId = Convert.ToInt32(found);
        }

        await using (var insert = new MySqlCommand(
            "INSERT IGNORE INTO trax_machine_sound_sets (machine_item_ref, slot, sound_set_item_ref) VALUES (@machine, @slot, @item);",
            connection, transaction))
        {
            insert.Parameters.AddWithValue("@machine", machineItemId);
            insert.Parameters.AddWithValue("@slot", slot);
            insert.Parameters.AddWithValue("@item", itemId);
            if (await insert.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                return null;
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return itemId;
    }

    public async Task<TraxMachineSoundSet?> EjectSoundSetAsync(int machineItemId, int slot, CancellationToken cancellationToken = default)
    {
        TraxMachineSoundSet? set = (await GetMachineSoundSetsAsync(machineItemId, cancellationToken)).FirstOrDefault(held => held.Slot == slot);
        if (set is null)
        {
            return null;
        }

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "DELETE FROM trax_machine_sound_sets WHERE machine_item_ref = @machine AND slot = @slot AND sound_set_item_ref = @item;", connection);
        command.Parameters.AddWithValue("@machine", machineItemId);
        command.Parameters.AddWithValue("@slot", slot);
        command.Parameters.AddWithValue("@item", set.ItemId);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0 ? set : null;
    }

    public async Task<int> BurnAsync(int userId, int songDiskDefinitionId, string extraData, int songId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        int diskId;
        await using (var item = new MySqlCommand(
            "INSERT INTO users_items (user_ref, ref_furni_def, extra_data, seen) VALUES (@user, @definition, @extraData, FALSE);", connection, transaction))
        {
            item.Parameters.AddWithValue("@user", userId);
            item.Parameters.AddWithValue("@definition", songDiskDefinitionId);
            item.Parameters.AddWithValue("@extraData", extraData);
            await item.ExecuteNonQueryAsync(cancellationToken);
            diskId = (int)item.LastInsertedId;
        }

        await using (var disk = new MySqlCommand(
            "INSERT INTO users_songs_disks (disk_item_ref, song_ref) VALUES (@disk, @song);", connection, transaction))
        {
            disk.Parameters.AddWithValue("@disk", diskId);
            disk.Parameters.AddWithValue("@song", songId);
            await disk.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return diskId;
    }

    public async Task<IReadOnlyList<TraxDiskItem>> GetInventoryDisksAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            $"""
            SELECT i.id, s.id, s.song_name, s.song_author, s.song_data
            FROM users_items i
            INNER JOIN users_songs_disks d ON d.disk_item_ref = i.id
            INNER JOIN users_songs s ON s.id = d.song_ref
            WHERE i.user_ref = @user AND i.room_ref IS NULL AND {ItemHolds.NotHeld("i.id")}
            ORDER BY i.id;
            """, connection);
        command.Parameters.AddWithValue("@user", userId);

        var disks = new List<TraxDiskItem>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            disks.Add(new TraxDiskItem(reader.GetInt32(0), new TraxSong(reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetString(4))));
        }

        return disks;
    }

    public async Task<IReadOnlyList<JukeboxDisk>> GetJukeboxDisksAsync(int jukeboxItemId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT h.slot, i.id, i.user_ref, s.id, s.song_name, s.song_author, s.song_data
            FROM jukebox_disks h
            INNER JOIN users_items i ON i.id = h.disk_item_ref
            INNER JOIN users_songs_disks d ON d.disk_item_ref = i.id
            INNER JOIN users_songs s ON s.id = d.song_ref
            WHERE h.machine_item_ref = @machine
            ORDER BY h.slot;
            """, connection);
        command.Parameters.AddWithValue("@machine", jukeboxItemId);

        var disks = new List<JukeboxDisk>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            disks.Add(new JukeboxDisk(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2),
                new TraxSong(reader.GetInt32(3), reader.GetString(4), reader.GetString(5), reader.GetString(6))));
        }

        return disks;
    }

    public async Task<bool> LoadDiskAsync(int jukeboxItemId, int slot, int userId, int diskItemId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var select = new MySqlCommand(
            $"""
            SELECT i.id
            FROM users_items i
            INNER JOIN users_songs_disks d ON d.disk_item_ref = i.id
            WHERE i.id = @disk AND i.user_ref = @user AND i.room_ref IS NULL AND {ItemHolds.NotHeld("i.id")}
            FOR UPDATE OF i;
            """, connection, transaction))
        {
            select.Parameters.AddWithValue("@disk", diskItemId);
            select.Parameters.AddWithValue("@user", userId);
            if (await select.ExecuteScalarAsync(cancellationToken) is null)
            {
                return false;
            }
        }

        await using (var insert = new MySqlCommand(
            "INSERT IGNORE INTO jukebox_disks (machine_item_ref, slot, disk_item_ref) VALUES (@machine, @slot, @disk);", connection, transaction))
        {
            insert.Parameters.AddWithValue("@machine", jukeboxItemId);
            insert.Parameters.AddWithValue("@slot", slot);
            insert.Parameters.AddWithValue("@disk", diskItemId);
            if (await insert.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                return false;
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<JukeboxDisk?> UnloadDiskAsync(int jukeboxItemId, int slot, CancellationToken cancellationToken = default)
    {
        JukeboxDisk? disk = (await GetJukeboxDisksAsync(jukeboxItemId, cancellationToken)).FirstOrDefault(loaded => loaded.Slot == slot);
        if (disk is null)
        {
            return null;
        }

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using (var command = new MySqlCommand(
            "DELETE FROM jukebox_disks WHERE machine_item_ref = @machine AND slot = @slot AND disk_item_ref = @disk;", connection, transaction))
        {
            command.Parameters.AddWithValue("@machine", jukeboxItemId);
            command.Parameters.AddWithValue("@slot", slot);
            command.Parameters.AddWithValue("@disk", disk.ItemId);
            if (await command.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                return null;
            }
        }

        await using (var playlist = new MySqlCommand("DELETE FROM jukebox_playlists WHERE disk_item_ref = @disk;", connection, transaction))
        {
            playlist.Parameters.AddWithValue("@disk", disk.ItemId);
            await playlist.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return disk;
    }

    public async Task<IReadOnlyList<int>> GetJukeboxPlaylistDiskIdsAsync(int jukeboxItemId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT disk_item_ref FROM jukebox_playlists WHERE machine_item_ref = @machine ORDER BY position;", connection);
        command.Parameters.AddWithValue("@machine", jukeboxItemId);
        return await ReadIdsAsync(command, cancellationToken);
    }

    public Task SetJukeboxPlaylistAsync(int jukeboxItemId, IReadOnlyList<int> diskItemIds, CancellationToken cancellationToken = default) =>
        ReplaceListAsync("jukebox_playlists", "disk_item_ref", jukeboxItemId, diskItemIds, cancellationToken);

    private async Task ReplaceListAsync(string table, string column, int machineItemId, IReadOnlyList<int> ids, CancellationToken cancellationToken)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using (var clear = new MySqlCommand($"DELETE FROM {table} WHERE machine_item_ref = @machine;", connection, transaction))
        {
            clear.Parameters.AddWithValue("@machine", machineItemId);
            await clear.ExecuteNonQueryAsync(cancellationToken);
        }

        for (int i = 0; i < ids.Count; i++)
        {
            await using var insert = new MySqlCommand(
                $"INSERT INTO {table} (machine_item_ref, position, {column}) VALUES (@machine, @position, @id);", connection, transaction);
            insert.Parameters.AddWithValue("@machine", machineItemId);
            insert.Parameters.AddWithValue("@position", i + 1);
            insert.Parameters.AddWithValue("@id", ids[i]);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task<IReadOnlyList<TraxSong>> ReadSongsAsync(MySqlCommand command, CancellationToken cancellationToken)
    {
        var songs = new List<TraxSong>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            songs.Add(new TraxSong(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
        }

        return songs;
    }

    private static async Task<IReadOnlyList<int>> ReadIdsAsync(MySqlCommand command, CancellationToken cancellationToken)
    {
        var ids = new List<int>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            ids.Add(reader.GetInt32(0));
        }

        return ids;
    }

    private static TraxSongEntry ReadEntry(MySqlDataReader reader) =>
        new(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetInt32(5) == 1);
}
