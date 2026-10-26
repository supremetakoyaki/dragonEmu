using DragonEmu.Domain;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;

namespace DragonEmu.Game.Settings;

public sealed class GameSettingsCache : IHostedService
{
    private readonly IGameSettingsRepository _repository;

    private volatile GameSettings _current = GameSettings.Default;

    public GameSettingsCache(IGameSettingsRepository repository)
    {
        _repository = repository;
    }

    public GameSettings Current => _current;

    public async Task SetInfobusOpenAsync(bool open, CancellationToken cancellationToken = default)
    {
        await _repository.SetInfobusOpenAsync(open, cancellationToken);
        _current = _current with { InfobusOpen = open };
    }

    public async Task ReloadAsync(CancellationToken cancellationToken = default) => _current = await _repository.GetAsync(cancellationToken);

    public Task StartAsync(CancellationToken cancellationToken) => ReloadAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
