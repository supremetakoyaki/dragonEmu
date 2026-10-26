using DragonEmu.Domain;
using DragonEmu.Persistence;

namespace DragonEmu.Game.Furniture;

public sealed class FurnitureAliasCache
{
    private readonly IFurnitureAliasRepository _repository;

    private volatile IReadOnlyList<FurnitureAlias> _aliases = [];

    public FurnitureAliasCache(IFurnitureAliasRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<FurnitureAlias> Aliases => _aliases;

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        _aliases = await _repository.GetAllEnabledAsync(cancellationToken);
    }
}
