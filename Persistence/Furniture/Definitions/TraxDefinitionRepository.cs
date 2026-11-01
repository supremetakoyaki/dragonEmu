using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class TraxDefinitionRepository : ITraxDefinitionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TraxDefinitionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<TraxSoundSet>> GetSoundSetsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TraxSample> samples = await GetSamplesAsync(cancellationToken);
        ILookup<int, int> samplesBySet = samples.OrderBy(sample => sample.Position).ToLookup(sample => sample.SoundSetId, sample => sample.Id);

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT ds.id, ds.furni_def_ref, fd.localization_name
            FROM definition_trax_sound_sets ds
            INNER JOIN definition_furniture fd ON fd.id = ds.furni_def_ref
            ORDER BY ds.id;
            """, connection);

        var sets = new List<TraxSoundSet>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            int id = reader.GetInt32(0);
            sets.Add(new TraxSoundSet(id, reader.GetInt32(1), reader.GetString(2), [.. samplesBySet[id]]));
        }

        return sets;
    }

    public async Task<IReadOnlyList<TraxSample>> GetSamplesAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT id, sound_set_ref, position, bars FROM definition_trax_samples;", connection);

        var samples = new List<TraxSample>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            samples.Add(new TraxSample(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3)));
        }

        return samples;
    }
}
