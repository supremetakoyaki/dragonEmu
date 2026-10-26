using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class GroupRepository : IGroupRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GroupRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GroupDetails?> GetDetailsAsync(int groupId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT g.id, g.name, g.description, g.ref_room, r.label
            FROM `groups` g
            LEFT JOIN rooms r ON r.id = g.ref_room
            WHERE g.id = @groupId;
            """, connection);
        command.Parameters.AddWithValue("@groupId", groupId);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? new GroupDetails(
                reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetInt32(3), reader.IsDBNull(4) ? string.Empty : reader.GetString(4))
            : null;
    }

    public async Task<IReadOnlyList<GroupBadge>> GetBadgesAsync(IReadOnlyCollection<int> groupIds, CancellationToken cancellationToken = default)
    {
        if (groupIds.Count == 0)
        {
            return [];
        }

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            $"SELECT id, badge FROM `groups` WHERE id IN ({string.Join(',', groupIds)});", connection);

        var badges = new List<GroupBadge>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            badges.Add(new GroupBadge(reader.GetInt32(0), reader.GetString(1)));
        }

        return badges;
    }
}
