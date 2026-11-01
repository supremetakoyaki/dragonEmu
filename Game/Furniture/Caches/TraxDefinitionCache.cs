using DragonEmu.Domain;
using DragonEmu.Persistence;

namespace DragonEmu.Game.Furniture;

public sealed class TraxDefinitionCache
{
    private readonly ITraxDefinitionRepository _repository;

    private volatile Snapshot _snapshot = new(new Dictionary<int, TraxSoundSet>(), new Dictionary<int, TraxSoundSet>(), new Dictionary<int, TraxSample>());

    public TraxDefinitionCache(ITraxDefinitionRepository repository)
    {
        _repository = repository;
    }

    public int Count => _snapshot.ById.Count;

    public TraxSoundSet? GetSoundSet(int id) => _snapshot.ById.GetValueOrDefault(id);

    public TraxSoundSet? GetSoundSetByFurni(int furniDefinitionId) => _snapshot.ByFurni.GetValueOrDefault(furniDefinitionId);

    public TraxSample? GetSample(int id) => _snapshot.Samples.GetValueOrDefault(id);

    public IReadOnlyDictionary<int, int> SamplesOf(IEnumerable<int> soundSetIds)
    {
        var samples = new Dictionary<int, int>();
        foreach (int setId in soundSetIds)
        {
            if (GetSoundSet(setId) is { } set)
            {
                foreach (int sampleId in set.SampleIds)
                {
                    samples[sampleId] = _snapshot.Samples[sampleId].Bars;
                }
            }
        }

        return samples;
    }

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TraxSoundSet> sets = await _repository.GetSoundSetsAsync(cancellationToken);
        IReadOnlyList<TraxSample> samples = await _repository.GetSamplesAsync(cancellationToken);
        _snapshot = new Snapshot(
            sets.ToDictionary(set => set.Id), sets.ToDictionary(set => set.FurniDefinitionId), samples.ToDictionary(sample => sample.Id));
    }

    private sealed record Snapshot(
        Dictionary<int, TraxSoundSet> ById, Dictionary<int, TraxSoundSet> ByFurni, Dictionary<int, TraxSample> Samples);
}
