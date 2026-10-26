using DragonEmu.Domain;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;

namespace DragonEmu.Game.Advertisement;

public sealed class InterstitialAdCache : IHostedService
{
    private readonly IInterstitialAdRepository _repository;

    private volatile IReadOnlyList<InterstitialAd> _ads = [];

    public InterstitialAdCache(IInterstitialAdRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<InterstitialAd> Ads => _ads;

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        _ads = await _repository.GetAllAsync(cancellationToken);
    }

    public Task StartAsync(CancellationToken cancellationToken) => ReloadAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
