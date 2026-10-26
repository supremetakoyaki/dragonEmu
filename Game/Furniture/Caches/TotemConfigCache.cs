using DragonEmu.Domain;
using DragonEmu.Persistence;

namespace DragonEmu.Game.Furniture;

public sealed class TotemConfigCache
{
    private readonly ITotemConfigRepository _repository;

    private volatile IReadOnlyDictionary<(int Leg, int Head, int Planet), TotemCombination> _combinations =
        new Dictionary<(int Leg, int Head, int Planet), TotemCombination>();

    public TotemConfigCache(ITotemConfigRepository repository)
    {
        _repository = repository;
    }

    public int Count => _combinations.Count;

    public TotemCombination? Find(int legState, int headState, int planetState) =>
        _combinations.GetValueOrDefault((legState, headState, planetState));

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TotemCombination> combinations = await _repository.GetAllAsync(cancellationToken);
        _combinations = combinations.ToDictionary(combination => (combination.LegState, combination.HeadState, combination.PlanetState));
    }
}
