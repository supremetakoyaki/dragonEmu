using DragonEmu.Domain;
using DragonEmu.Persistence;

namespace DragonEmu.Game.Furniture;

public sealed class FurnitureDefinitionCache
{
    private readonly IFurnitureDefinitionRepository _repository;

    private volatile Snapshot _snapshot = new(new Dictionary<int, FurnitureDefinition>(), new Dictionary<string, FurnitureDefinition>());

    public FurnitureDefinitionCache(IFurnitureDefinitionRepository repository)
    {
        _repository = repository;
    }

    public int Count => _snapshot.ById.Count;

    public FurnitureDefinition? GetById(int id) => _snapshot.ById.GetValueOrDefault(id);

    public FurnitureDefinition? GetByClassname(string classname) => _snapshot.ByClassname.GetValueOrDefault(classname);

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FurnitureDefinition> definitions = await _repository.GetAllAsync(cancellationToken);

        var byClassname = new Dictionary<string, FurnitureDefinition>(StringComparer.OrdinalIgnoreCase);
        foreach (FurnitureDefinition definition in definitions)
        {
            byClassname.TryAdd(definition.Classname, definition);
        }

        _snapshot = new Snapshot(definitions.ToDictionary(definition => definition.Id), byClassname);
    }

    private sealed record Snapshot(Dictionary<int, FurnitureDefinition> ById, Dictionary<string, FurnitureDefinition> ByClassname);
}
