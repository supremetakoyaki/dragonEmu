using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class RecyclerPrizeRepository : IRecyclerPrizeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RecyclerPrizeRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<RecyclerLevel>> GetLevelsAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT l.level, l.odds, d.id, d.type, d.ref_furnidata
            FROM definition_catalog_recycler_levels l
            LEFT JOIN definition_catalog_recycler_prizes p ON p.level = l.level
            LEFT JOIN definition_furniture d ON d.id = p.ref_furni_def
            ORDER BY l.level DESC, d.classname;
            """, connection);

        var levels = new List<(int Level, int Odds, List<RecyclerPrize> Prizes)>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            int level = reader.GetInt32(0);
            if (levels.Count == 0 || levels[^1].Level != level)
            {
                levels.Add((level, reader.GetInt32(1), []));
            }

            if (!reader.IsDBNull(2))
            {
                levels[^1].Prizes.Add(new RecyclerPrize(
                    reader.GetInt32(2), reader.GetString(3) == "s" ? FurnitureType.Floor : FurnitureType.Wall, reader.GetInt32(4)));
            }
        }

        return levels.Select(level => new RecyclerLevel(level.Level, level.Odds, level.Prizes)).ToList();
    }
}
