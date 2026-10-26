using DragonEmu.Domain;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Achievements;

public sealed class AchievementDefinitionCache : IHostedService
{
    private readonly IAchievementDefinitionRepository _repository;
    private readonly ILogger<AchievementDefinitionCache> _logger;

    private volatile IReadOnlyDictionary<string, AchievementDefinition> _byCodeBase =
        new Dictionary<string, AchievementDefinition>();

    public AchievementDefinitionCache(IAchievementDefinitionRepository repository, ILogger<AchievementDefinitionCache> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public IReadOnlyCollection<AchievementDefinition> Definitions => (IReadOnlyCollection<AchievementDefinition>)_byCodeBase.Values;

    public AchievementDefinition? GetByCodeBase(string codeBase) => _byCodeBase.GetValueOrDefault(codeBase);

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AchievementDefinition> all = await _repository.GetAllAsync(cancellationToken);
        _byCodeBase = all.ToDictionary(a => a.CodeBase, a => a);
        _logger.LogInformation("Achievement definition cache loaded: {Count} achievements.", all.Count);
    }

    public Task StartAsync(CancellationToken cancellationToken) => ReloadAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
