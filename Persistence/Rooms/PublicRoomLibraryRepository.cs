using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class PublicRoomLibraryRepository : IPublicRoomLibraryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PublicRoomLibraryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<PublicRoomLibraries>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT h.label, l.library_name, l.nav_code FROM definition_public_room_libraries l
            INNER JOIN definition_room_heightmap h ON h.id = l.heightmap_ref;
            """, connection);

        var libraries = new List<PublicRoomLibraries>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            libraries.Add(new PublicRoomLibraries(reader.GetString(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2)));
        }

        return libraries;
    }
}
