using DragonEmu.Domain;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Catalog;

public sealed class RecyclerPrizeCache : IHostedService
{
    private readonly IRecyclerPrizeRepository _repository;
    private readonly ILogger<RecyclerPrizeCache> _logger;

    private volatile IReadOnlyList<RecyclerLevel> _levels = [];

    public RecyclerPrizeCache(IRecyclerPrizeRepository repository, ILogger<RecyclerPrizeCache> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public IReadOnlyList<RecyclerLevel> Levels => _levels;

    public RecyclerPrize? Draw(Random random) => Draw(_levels, random);

    public static RecyclerPrize? Draw(IReadOnlyList<RecyclerLevel> levels, Random random)
    {
        foreach (RecyclerLevel level in levels.OrderByDescending(level => level.Odds))
        {
            if (level.Prizes.Count > 0 && random.Next(level.Odds) == 0)
            {
                return level.Prizes[random.Next(level.Prizes.Count)];
            }
        }

        return null;
    }

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        _levels = await _repository.GetLevelsAsync(cancellationToken);
        _logger.LogInformation(
            "Recycler prizes loaded: {LevelCount} levels, {PrizeCount} prizes.", _levels.Count, _levels.Sum(level => level.Prizes.Count));
    }

    public Task StartAsync(CancellationToken cancellationToken) => ReloadAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
