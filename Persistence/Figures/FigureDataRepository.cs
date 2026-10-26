using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class FigureDataRepository : IFigureDataRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public FigureDataRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<FigureSettype>> GetSettypesAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT type, palette_ref, mandatory FROM definition_figure_settypes;", connection);

        var settypes = new List<FigureSettype>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            settypes.Add(new FigureSettype(reader.GetString(0), reader.GetInt32(1), reader.GetByte(2) != 0));
        }

        return settypes;
    }

    public async Task<IReadOnlyList<FigureSet>> GetSetsAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT id, settype_ref, gender, club, colorable, selectable FROM definition_figure_sets;", connection);

        var sets = new List<FigureSet>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            sets.Add(new FigureSet(
                reader.GetInt32(0), reader.GetString(1), reader.GetString(2)[0],
                reader.GetByte(3) != 0, reader.GetByte(4) != 0, reader.GetByte(5) != 0));
        }

        return sets;
    }

    public async Task<IReadOnlyList<FigureColor>> GetColorsAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT id, palette_ref, hex, club, selectable FROM definition_figure_colors;", connection);

        var colors = new List<FigureColor>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            colors.Add(new FigureColor(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetByte(3) != 0, reader.GetByte(4) != 0));
        }

        return colors;
    }
}
