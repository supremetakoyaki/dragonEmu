using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Furniture;

public sealed class FurnitureCachesLoader : IHostedService
{
    private readonly FurnitureDefinitionCache _definitions;
    private readonly TotemConfigCache _totemConfig;
    private readonly FurnitureAliasCache _aliases;
    private readonly TraxDefinitionCache _trax;
    private readonly ILogger<FurnitureCachesLoader> _logger;

    public FurnitureCachesLoader(
        FurnitureDefinitionCache definitions, TotemConfigCache totemConfig, FurnitureAliasCache aliases, TraxDefinitionCache trax,
        ILogger<FurnitureCachesLoader> logger)
    {
        _definitions = definitions;
        _totemConfig = totemConfig;
        _aliases = aliases;
        _trax = trax;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _definitions.ReloadAsync(cancellationToken);
        await _totemConfig.ReloadAsync(cancellationToken);
        await _aliases.ReloadAsync(cancellationToken);
        await _trax.ReloadAsync(cancellationToken);

        _logger.LogInformation(
            "Furniture definition cache loaded: {Definitions} definitions, {Aliases} furni aliases, {SoundSets} trax sound sets.",
            _definitions.Count, _aliases.Aliases.Count, _trax.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
