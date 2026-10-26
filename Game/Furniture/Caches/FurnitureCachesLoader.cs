using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Furniture;

public sealed class FurnitureCachesLoader : IHostedService
{
    private readonly FurnitureDefinitionCache _definitions;
    private readonly TotemConfigCache _totemConfig;
    private readonly FurnitureAliasCache _aliases;
    private readonly ILogger<FurnitureCachesLoader> _logger;

    public FurnitureCachesLoader(
        FurnitureDefinitionCache definitions, TotemConfigCache totemConfig, FurnitureAliasCache aliases, ILogger<FurnitureCachesLoader> logger)
    {
        _definitions = definitions;
        _totemConfig = totemConfig;
        _aliases = aliases;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _definitions.ReloadAsync(cancellationToken);
        await _totemConfig.ReloadAsync(cancellationToken);
        await _aliases.ReloadAsync(cancellationToken);

        _logger.LogInformation(
            "Furniture definition cache loaded: {Definitions} definitions, {Aliases} furni aliases.",
            _definitions.Count, _aliases.Aliases.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
